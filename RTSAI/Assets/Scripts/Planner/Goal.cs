namespace RTS.AI.Planner
{
    [System.Serializable]
    public class Goal<T> where T : System.Enum
    {
        public T Target;
        public bool Value;
    }
}