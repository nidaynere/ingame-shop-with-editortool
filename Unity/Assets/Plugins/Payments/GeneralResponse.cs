namespace Payments
{
    [System.Serializable]
    public class GeneralResponse
    {
        public int error;
        public string message;

        public bool isError => error > 0;
    }
}
