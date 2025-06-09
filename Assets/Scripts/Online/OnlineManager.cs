using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestiona la conexión online punto a punto entre dos jugadores mediante un código de 5 caracteres.
/// El host crea una sala y comparte el código con el invitado.
/// El invitado introduce ese código para descubrir y conectarse al host.
/// Funciona en LAN utilizando broadcast UDP para descubrir la partida.
/// </summary>
public class OnlineManager : MonoBehaviour
{
    public int tcpPort = 7777;
    public int discoveryPort = 8000;
    public UnityEvent OnConnectionEstablished;

    // Elementos de interfaz
    public GameObject[] botones;
    public TextMeshProUGUI codigoUnion;
    public GameObject panelCrearSala;
    public GameObject panelEsperando;
    public GameObject panelIntroducirCodigo;
    public TextMeshProUGUI textoEstado;
    public Button crearPartidaPrivada;
    public Button unirsePartidaPrivada;
    public TMP_InputField inputCodigo;
    public Button botonEnviarUnion;

    TcpListener hostListener;
    TcpClient tcpClient;
    UdpClient udpClient;

    bool isHost;
    string joinCode;
    IPEndPoint broadcastEndPoint;

    void Start()
    {
        if (crearPartidaPrivada != null)
            crearPartidaPrivada.onClick.AddListener(() => CreateRoom());
        if (unirsePartidaPrivada != null)
            unirsePartidaPrivada.onClick.AddListener(OnJoinRoomButton);
        if (botonEnviarUnion != null)
            botonEnviarUnion.onClick.AddListener(SendJoin);

        // Asegurar que los subpaneles están ocultos al inicio
        if (panelEsperando != null)
            panelEsperando.SetActive(false);
        if (panelIntroducirCodigo != null)
            panelIntroducirCodigo.SetActive(false);
    }

    void Awake()
    {
        broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
    }

    /// <summary>
    /// Crea una sala y genera un código aleatorio de 5 caracteres.
    /// Empieza a escuchar conexiones entrantes y a anunciar la sala por broadcast.
    /// </summary>
    public string CreateRoom()
    {
        isHost = true;
        joinCode = GenerateCode();

        hostListener = new TcpListener(IPAddress.Any, tcpPort);
        hostListener.Start();
        hostListener.BeginAcceptTcpClient(OnClientConnected, null);

        udpClient = new UdpClient(discoveryPort);
        udpClient.EnableBroadcast = true;
        StartCoroutine(HostBroadcastRoutine());

        Debug.Log($"Sala creada con código {joinCode}");

        // Actualizar UI de espera del anfitrión
        if (panelCrearSala != null)
            panelCrearSala.SetActive(false);
        if (panelEsperando != null)
            panelEsperando.SetActive(true);
        if (codigoUnion != null)
            codigoUnion.text = joinCode;
        if (textoEstado != null)
            textoEstado.text = "Esperando al otro jugador";
        return joinCode;
    }

    IEnumerator HostBroadcastRoutine()
    {
        var receiveEndPoint = new IPEndPoint(IPAddress.Any, discoveryPort);
        while (isHost)
        {
            // Anunciar la existencia del host periódicamente
            byte[] data = Encoding.UTF8.GetBytes($"HOST:{joinCode}");
            udpClient.Send(data, data.Length, broadcastEndPoint);

            // Revisar solicitudes de unión
            if (udpClient.Available > 0)
            {
                byte[] msg = udpClient.Receive(ref receiveEndPoint);
                string text = Encoding.UTF8.GetString(msg);
                if (text == $"JOIN:{joinCode}")
                {
                    string ip = GetLocalIPAddress();
                    byte[] reply = Encoding.UTF8.GetBytes($"ACCEPT:{joinCode}:{ip}");
                    udpClient.Send(reply, reply.Length, receiveEndPoint);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnClientConnected(IAsyncResult ar)
    {
        tcpClient = hostListener.EndAcceptTcpClient(ar);
        StartCoroutine(HandleConnection(tcpClient, true));
    }

    /// <summary>
    /// Se intenta unir a una sala existente con el código proporcionado.
    /// </summary>
    public void JoinRoom(string code)
    {
        isHost = false;
        joinCode = code;

        udpClient = new UdpClient(discoveryPort);
        udpClient.EnableBroadcast = true;

        SendJoinRequest();
        StartCoroutine(ClientListenRoutine());
    }

    void SendJoinRequest()
    {
        byte[] data = Encoding.UTF8.GetBytes($"JOIN:{joinCode}");
        udpClient.Send(data, data.Length, broadcastEndPoint);
    }

    IEnumerator ClientListenRoutine()
    {
        var endPoint = new IPEndPoint(IPAddress.Any, discoveryPort);
        float timeout = 5f;
        float timer = 0f;
        while (tcpClient == null && timer < timeout)
        {
            if (udpClient.Available > 0)
            {
                var data = udpClient.Receive(ref endPoint);
                string message = Encoding.UTF8.GetString(data);
                if (message.StartsWith($"ACCEPT:{joinCode}:"))
                {
                    string[] parts = message.Split(':');
                    if (parts.Length >= 3)
                    {
                        ConnectToHost(parts[2]);
                        yield break;
                    }
                }
            }
            yield return null;
            timer += Time.deltaTime;
        }
        Debug.LogWarning($"No se encontró una sala con el código {joinCode}");
    }

    void ConnectToHost(string ip)
    {
        tcpClient = new TcpClient();
        tcpClient.BeginConnect(IPAddress.Parse(ip), tcpPort, ar =>
        {
            tcpClient.EndConnect(ar);
            StartCoroutine(HandleConnection(tcpClient, false));
        }, null);
    }

    // --- UI helpers ---

    /// <summary>
    /// Muestra el campo de entrada para unirse a una sala y activa el teclado.
    /// </summary>
    public void OnJoinRoomButton()
    {
        if (panelCrearSala != null)
            panelCrearSala.SetActive(false);
        if (panelIntroducirCodigo != null)
            panelIntroducirCodigo.SetActive(true);
        if (panelEsperando != null)
            panelEsperando.SetActive(false);

        if (inputCodigo != null)
        {
            inputCodigo.gameObject.SetActive(true);
            inputCodigo.text = string.Empty;
            inputCodigo.ActivateInputField();
        }

        if (botonEnviarUnion != null)
            botonEnviarUnion.gameObject.SetActive(true);

        if (textoEstado != null)
            textoEstado.text = "Introduce el c\u00F3digo";
    }

    /// <summary>
    /// Envía la solicitud de unión usando el código introducido en el campo.
    /// </summary>
    public void SendJoin()
    {
        if (inputCodigo != null)
            JoinRoom(inputCodigo.text);
    }

    IEnumerator HandleConnection(TcpClient client, bool hostSide)
    {
        NetworkStream stream = client.GetStream();

        if (hostSide)
        {
            // enviar código y esperar confirmación
            byte[] codeData = Encoding.UTF8.GetBytes(joinCode);
            stream.Write(codeData, 0, codeData.Length);

            byte[] buffer = new byte[16];
            int len = stream.Read(buffer, 0, buffer.Length);
            string confirm = Encoding.UTF8.GetString(buffer, 0, len);
            if (confirm != joinCode)
            {
                Debug.LogWarning("El cliente envió un código incorrecto");
                client.Close();
                yield break;
            }
        }
        else
        {
            // recibir código y responder
            byte[] buffer = new byte[16];
            int len = stream.Read(buffer, 0, buffer.Length);
            string code = Encoding.UTF8.GetString(buffer, 0, len);
            if (code != joinCode)
            {
                Debug.LogWarning("El host envió un código incorrecto");
                client.Close();
                yield break;
            }
            byte[] data = Encoding.UTF8.GetBytes(joinCode);
            stream.Write(data, 0, data.Length);
        }

        Debug.Log("Conexión establecida correctamente");
        OnConnectionEstablished?.Invoke();

        while (client.Connected)
        {
            yield return null;
        }
    }

    string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var rand = new System.Random();
        return new string(Enumerable.Repeat(chars, 5).Select(s => s[rand.Next(s.Length)]).ToArray());
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "127.0.0.1";
    }

    /// <summary>
    /// Abre el panel de partida privada y desactiva los botones de inicio.
    /// </summary>
    public void PrivatePartClick()
    {
        if (botones != null)
        {
            foreach (var b in botones)
                b.SetActive(false);
        }

        if (panelCrearSala != null)
            panelCrearSala.SetActive(true);
        if (panelEsperando != null)
            panelEsperando.SetActive(false);
        if (panelIntroducirCodigo != null)
            panelIntroducirCodigo.SetActive(false);
    }

    /// <summary>
    /// Cierra el panel de partida privada volviendo a mostrar los botones de inicio.
    /// </summary>
    public void ClosePrivatePanel()
    {
        ReturnToMainMenu();
    }

    /// <summary>
    /// Cierra todos los paneles online y reactiva los botones principales.
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (panelCrearSala != null)
            panelCrearSala.SetActive(false);
        if (panelEsperando != null)
            panelEsperando.SetActive(false);
        if (panelIntroducirCodigo != null)
            panelIntroducirCodigo.SetActive(false);

        if (botones != null)
        {
            foreach (var b in botones)
                b.SetActive(true);
        }
        if (inputCodigo != null)
            inputCodigo.text = string.Empty;
        if (botonEnviarUnion != null)
            botonEnviarUnion.gameObject.SetActive(false);
    }

    void OnApplicationQuit()
    {
        hostListener?.Stop();
        tcpClient?.Close();
        udpClient?.Close();
    }
}