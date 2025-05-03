// definimos posibles estados del juego, util hacerlo asi por que es escalable y en el futuro podremos añadir facilmente mas estados (ej: estableciendo conexion con otro jugador... abriendo cofre... error de conexion...)
public enum GameState
{
    Menu,
    Playing,
    Paused,
    Ended
}
