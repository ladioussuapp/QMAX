
namespace Com4Love.Qmax
{
    public struct Position
    {
        public int Row;
        public int Col;

        public Position(int Row, int Col)
        {
            this.Row = Row;
            this.Col = Col;
        }


        public override string ToString()
        {
            return string.Format("[{0}.{1}]", Row, Col);
        }

        static public Position operator +(Position v1, Position v2)
        {
            return new Position(v1.Col + v2.Col , v1.Row + v2.Row);
        }

        static public bool operator ==(Position v1, Position v2)
        {
            return v1.Row == v2.Row && v1.Col == v2.Col;
        }
        static public bool operator !=(Position v1, Position v2)
        {
            return !(v1 == v2);
        }


        public override bool Equals(System.Object obj)
        {
            if (obj is Position)
            {
                Position p = (Position)obj;
                return p.Row == Row && p.Col == Col;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Row.GetHashCode() ^ Col.GetHashCode();
        }
    }
}
