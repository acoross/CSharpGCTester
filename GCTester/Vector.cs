
namespace GCTester
{
    class Vector
    {
        public double X;
        public double Y;

        public Vector() { }

        public Vector(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static double SqDist(Vector v1, Vector v2)
        {
            var diffX = (v1.X - v2.X);
            var diffY = (v1.Y - v2.Y);

            return diffX * diffX + diffY * diffY;
        }
    }
}
