using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine.Lib
{
    public class BezierCurve
    {
        private static double[] FactorialLookup;

        static BezierCurve()
        {
            CreateFactorialTable();
        }

        // just check if n is appropriate, then return the result
        private static double factorial(int n)
        {
            if (n < 0) { throw new Exception("n is less than 0"); }
            if (n > 32) { throw new Exception("n is greater than 32"); }

            return FactorialLookup[n]; /* returns the value n! as a SUMORealing point number */
        }

        // create lookup table for fast factorial calculation
        private static void CreateFactorialTable()
        {
            // fill untill n=32. The rest is too high to represent
            double[] a = new double[33];
            a[0] = 1.0;
            a[1] = 1.0;
            a[2] = 2.0;
            a[3] = 6.0;
            a[4] = 24.0;
            a[5] = 120.0;
            a[6] = 720.0;
            a[7] = 5040.0;
            a[8] = 40320.0;
            a[9] = 362880.0;
            a[10] = 3628800.0;
            a[11] = 39916800.0;
            a[12] = 479001600.0;
            a[13] = 6227020800.0;
            a[14] = 87178291200.0;
            a[15] = 1307674368000.0;
            a[16] = 20922789888000.0;
            a[17] = 355687428096000.0;
            a[18] = 6402373705728000.0;
            a[19] = 121645100408832000.0;
            a[20] = 2432902008176640000.0;
            a[21] = 51090942171709440000.0;
            a[22] = 1124000727777607680000.0;
            a[23] = 25852016738884976640000.0;
            a[24] = 620448401733239439360000.0;
            a[25] = 15511210043330985984000000.0;
            a[26] = 403291461126605635584000000.0;
            a[27] = 10888869450418352160768000000.0;
            a[28] = 304888344611713860501504000000.0;
            a[29] = 8841761993739701954543616000000.0;
            a[30] = 265252859812191058636308480000000.0;
            a[31] = 8222838654177922817725562880000000.0;
            a[32] = 263130836933693530167218012160000000.0;
            FactorialLookup = a;
        }

        private static double Ni(int n, int i)
        {
            double ni;
            double a1 = factorial(n);
            double a2 = factorial(i);
            double a3 = factorial(n - i);
            ni = a1 / (a2 * a3);
            return ni;
        }

        // Calculate Bernstein basis
        private static double Bernstein(int n, int i, double t)
        {
            double basis;
            double ti; /* t^i */
            double tni; /* (1 - t)^i */

            /* Prevent problems with pow */

            if (t == 0.0 && i == 0)
                ti = 1.0;
            else
                ti = Math.Pow(t, i);

            if (n == i && t == 1.0)
                tni = 1.0;
            else
                tni = Math.Pow((1 - t), (n - i));

            //Bernstein basis
            basis = Ni(n, i) * ti * tni;
            return basis;
        }

        /// <param name="inPoints">传入的点坐标[x,y]</param>
        /// <param name="outPoints">输出的点坐标[x,y]</param>
        public static void Bezier2D(double[] inPoints, double[] outPoints)
        {
            int inPointCount = (inPoints.Length) / 2;
            int outPointCount = (outPoints.Length) / 2;
            int icount, jcount;
            double step, t;

            // Calculate points on curve

            icount = 0;
            t = 0;
            step = (double)1.0 / (outPointCount - 1);

            for (int i1 = 0; i1 != outPointCount; i1++)
            {
                if ((1.0 - t) < 5e-6)
                    t = 1.0;

                jcount = 0;
                outPoints[icount] = 0.0;
                outPoints[icount + 1] = 0.0;
                for (int i = 0; i != inPointCount; i++)
                {
                    double basis = Bernstein(inPointCount - 1, i, t);
                    outPoints[icount] += basis * inPoints[jcount];
                    outPoints[icount + 1] += basis * inPoints[jcount + 1];
                    jcount = jcount + 2;
                }

                icount += 2;
                t += step;
            }
        }

        public static List<Vector2> Bezier2D(List<Vector2> inPoints, int outPointsCount)
        {
            var inP = new double[inPoints.Count * 2];
            var outP = new double[outPointsCount * 2];
            for (var i = 0; i < inPoints.Count; i++)
            {
                inP[i * 2] = inPoints[i].X;
                inP[i * 2 + 1] = inPoints[i].Y;
            }
            Bezier2D(inP, outP);
            var ol = new List<Vector2>();
            for (var i = 0; i < outPointsCount; i++)
            {
                ol.Add(new Vector2((float)outP[i*2], (float)outP[i*2 + 1]));
            }

            return ol;
        }
    }
}
