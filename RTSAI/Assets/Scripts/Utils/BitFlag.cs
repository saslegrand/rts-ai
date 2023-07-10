namespace SL.Tools
{
    public class BitFlag
    {
        private int _flags;

        public BitFlag() {}

        public BitFlag(int in_flags)
        {
            Enable(in_flags);
        }

        public BitFlag(BitFlag in_bitFlag)
        {
            _flags = in_bitFlag._flags;
        }

        public void Enable(int in_flag)
        {
            _flags |= in_flag;
        }
    
        public void Disable(int in_flag)
        {
            _flags &= ~in_flag;
        }

        public void SetFlag(int in_flag, bool in_set)
        {
            if (in_set)
                Enable(in_flag);
            else
                Disable(in_flag);
        }

        public bool IsEnable(int in_flag)
        {
            int flagToInt = in_flag;
            return (_flags & flagToInt) == flagToInt;
        }

        public bool Equal(BitFlag in_bitFlag)
        {
            return _flags == in_bitFlag._flags;
        }
    }
}