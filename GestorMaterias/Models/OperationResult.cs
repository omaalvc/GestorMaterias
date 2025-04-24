namespace GestorMaterias.Models
{
    /// <summary>
    /// Clase utilizada para devolver resultados de operaciones de servicios
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static OperationResult Ok(string message = "Operación realizada con éxito", object data = null)
        {
            return new OperationResult 
            { 
                Success = true, 
                Message = message,
                Data = data
            };
        }

        public static OperationResult Error(string message = "Ha ocurrido un error", object data = null)
        {
            return new OperationResult 
            { 
                Success = false, 
                Message = message,
                Data = data
            };
        }
    }
}