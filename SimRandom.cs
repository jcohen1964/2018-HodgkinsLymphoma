// Downloaded from 
// http://www.cambiaresearch.com/c4/e4d91750-f604-436d-97c9-20de3e28521c/csharp-RandomProvider-Class.aspx
// May 18, 2010

// Added functions:
//      uniform(a,b)
//      normal(mu,sigma)
//      Bernoulli(p)

using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRandom
{

    /// <summary>
    /// RandomProvider.  Provides random numbers of all data types
    /// in specified ranges.  It also contains a couple of methods
    /// from Normally (Gaussian) distributed random numbers and 
    /// Exponentially distributed random numbers.
    /// </summary>
    public class RandomProvider
    {
        private static Random m_RNG1;
        private static double m_StoredUniformDeviate;
        private static bool m_StoredUniformDeviateIsGood = false;

        #region -- Construction/Initialization --

        static RandomProvider()
        {
            Reset();
        }
        public static void Reset()
        {
            m_RNG1 = new Random(Environment.TickCount);
        }

        #endregion

        #region -- Uniform Deviates --

        /// <summary>
        /// Returns double in the range [0, 1)
        /// </summary>
        public static double Next()
        {
            double returnValue = m_RNG1.NextDouble();
            return returnValue;
        }

        /// <summary>
        /// Returns true or false randomly.
        /// </summary>
        public static bool NextBoolean()
        {
            if (m_RNG1.Next(0, 2) == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Returns double in the range [0, 1)
        /// </summary>
        public static double NextDouble()
        {
            double rn = m_RNG1.NextDouble();
            return rn;
        }

        /// <summary>
        /// Returns Int16 in the range [min, max)
        /// </summary>
        public static Int16 Next(Int16 min, Int16 max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }
            double rn = (max * 1.0 - min * 1.0) * m_RNG1.NextDouble() + min * 1.0 - 0.5;
            return Convert.ToInt16(rn);
        }

        /// <summary>
        /// Returns Int32 in the range [min, max)
        /// </summary>
        public static int Next(int min, int max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }
            double rn = (max * 1.0 - min * 1.0) * m_RNG1.NextDouble() + min * 1.0 - 0.5;
            return Convert.ToInt32(rn);
        }

        /// <summary>
        /// Returns Int64 in the range [min, max)
        /// </summary>
        public static Int64 Next(Int64 min, Int64 max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            double rn = (max * 1.0 - min * 1.0) * m_RNG1.NextDouble() + min * 1.0 - 0.5;
            return Convert.ToInt64(rn);
        }

        /// <summary>
        /// Returns float (Single) in the range [min, max)
        /// </summary>
        public static Single Next(Single min, Single max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            double rn = (max * 1.0 - min * 1.0) * m_RNG1.NextDouble() + min * 1.0;
            return Convert.ToSingle(rn);
        }

        /// <summary>
        /// Returns double in the range [min, max)
        /// </summary>
        public static double Next(double min, double max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            double rn = (max - min) * m_RNG1.NextDouble() + min;
            return rn;
        }

        /// <summary>
        /// Returns DateTime in the range [min, max)
        /// </summary>
        public static DateTime Next(DateTime min, DateTime max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }
            long minTicks = min.Ticks;
            long maxTicks = max.Ticks;
            double rn = (Convert.ToDouble(maxTicks)
               - Convert.ToDouble(minTicks)) * m_RNG1.NextDouble()
               + Convert.ToDouble(minTicks);
            return new DateTime(Convert.ToInt64(rn));
        }

        /// <summary>
        /// Returns TimeSpan in the range [min, max)
        /// </summary>
        public static TimeSpan Next(TimeSpan min, TimeSpan max)
        {
            if (max <= min)
            {
                string message = "Max must be greater than min.";
                throw new ArgumentException(message);
            }

            long minTicks = min.Ticks;
            long maxTicks = max.Ticks;
            double rn = (Convert.ToDouble(maxTicks)
               - Convert.ToDouble(minTicks)) * m_RNG1.NextDouble()
               + Convert.ToDouble(minTicks);
            return new TimeSpan(Convert.ToInt64(rn));
        }

        /// <summary>
        /// Returns boolean TRUE with probability p, false otherwise
        /// </summary>
        public static bool NextBool(float p)
        {
            return NextUniform() < p;
        }

        /// <summary>
        /// Returns double in the range [min, max)
        /// </summary>
        public static double NextUniform()
        {
            return Next();
        }

        /// <summary>
        /// Returns a uniformly random integer representing one of the values 
        /// in the enum.
        /// </summary>
        public static int NextEnum(Type enumType)
        {
            int[] values = (int[])Enum.GetValues(enumType);
            int randomIndex = Next(0, values.Length);
            return values[randomIndex];
        }

        #endregion

        #region -- Exponential Deviates --

        /// <summary>
        /// Returns an exponentially distributed, positive, random deviate 
        /// of unit mean.
        /// </summary>
        public static double NextExponential()
        {
            double dum = 0.0;
            while (dum == 0.0)
                dum = NextUniform();
            return -1.0 * System.Math.Log(dum, System.Math.E);
        }

        #endregion

        #region -- Normal Deviates --

        /// <summary>
        /// Returns a normally distributed deviate with zero mean and unit 
        /// variance.
        /// </summary>
        public static double NextNormal()
        {
            // based on algorithm from Numerical Recipes
            if (m_StoredUniformDeviateIsGood)
            {
                m_StoredUniformDeviateIsGood = false;
                return m_StoredUniformDeviate;
            }
            else
            {
                double rsq = 0.0;
                double v1 = 0.0, v2 = 0.0, fac = 0.0;
                while (rsq >= 1.0 || rsq == 0.0)
                {
                    v1 = 2.0 * Next() - 1.0;
                    v2 = 2.0 * Next() - 1.0;
                    rsq = v1 * v1 + v2 * v2;
                }
                fac = System.Math.Sqrt(-2.0
                   * System.Math.Log(rsq, System.Math.E) / rsq);
                m_StoredUniformDeviate = v1 * fac;
                m_StoredUniformDeviateIsGood = true;
                return v2 * fac;
            }
        }

        #endregion

        #region - Parameterized uniform --
        public static double NextUniform(double a, double b)
        {
            return a + (b - a) * NextUniform();
        }
        #endregion


        #region -- Parameterized normal --
        // Returns normal with mean mu and std sigma
        public static double NextNormal(double mu, double sigma)
        {
            return mu + sigma * NextNormal();
        }
        #endregion

        #region -- Parameterized and bounded normal --
        // Returns normal with mean mu and std sigma bounded by min and max
        // Normal is resampled until bounds are satisfied.
        public static double NextNormal(double mu, double sigma, double min, double max)
        {
            const int MAXITER = 1000;         // Max times we can try to find a value between min and max
            double returnValue;

            int i = 0;
            if (max <= min) { return mu; }    // max does not exceed min - invalid
            do
            {
                returnValue = NextNormal(mu, sigma);
                i++;
            }
            while ((returnValue < min || returnValue > max) && i < MAXITER);

            if (returnValue < min || returnValue > max)
                returnValue = (max + min) / 2;
            return returnValue;
        }

        #endregion

        #region -- Bernoulli --
        public static bool NextBernoulli(double p)
        {
            return (NextUniform() < p);
        }
        #endregion

        /// <summary>
        /// Returns the next event to occur out of a set of events, all of which
        /// have an exponential distribution for the delay until the next occurance
        /// Takes a list of rates as arguments, ignoring non-positive values.  Randomly
        /// selects an event time corresponding to each valid rate.  
        /// Returns an array with (1) an integer (starting at 0) indicating which 
        /// event occurred first, and (2) the time at which the event occurred
        /// </summary>
        public static decimal[] NextExpEvent(List<decimal> lambdaList)
        {
            int nextEventNum = -1;                      // Next event to occur
            int nextEventNum_maybe = -1;                // Next event tested
            decimal nextEventTime = Decimal.MaxValue;   // Time of next event to occur
            foreach (decimal lambda in lambdaList)      // Loop through all event rates
            {
                nextEventNum_maybe++;                   // Increment the event counter
                if (lambda > 0)                         // Test only if rate > 0
                {
                    decimal nextEventTime_maybe = (decimal)NextExponential() / lambda;
                    // Grab this event if it occurs first
                    if (nextEventTime_maybe <= nextEventTime)
                    {
                        nextEventTime = nextEventTime_maybe;
                        nextEventNum = nextEventNum_maybe;
                    }
                }
            }
            decimal[] returnValue = new decimal[2];
            returnValue[0] = (decimal)nextEventNum;    // Store event number
            returnValue[1] = nextEventTime;             // Store event time
            return returnValue;
        }
    }
}
