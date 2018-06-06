using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL1
{
    class Utility
    {
        //****************************************************************************************
        //****************************************************************************************
        // UTILITY FUNCTIONS
        //****************************************************************************************
        //****************************************************************************************
        
        //****************************************************************************************
        // Takes a probability and returns a rate
        //****************************************************************************************
        public static double rateFromProb(double p)
        {
            double r = -Math.Log(1 - p);
            return r;
        }

        //****************************************************************************************
        // Takes a rate and returns a probability
        //****************************************************************************************
        public static double probFromRate(double r)
        {
            double p = 1 - Math.Exp(-r);
            return p;
        }

        //****************************************************************************************
        // Computes discounted QALYs accrued as a function of timestep, utility weight, and time 
        // since start of simulation.
        //****************************************************************************************
        public static double deltaQALY(double timestep, double util, double timeSinceStart)
        {
            double t = (0.5 * timestep) + timeSinceStart;
            double dr = K.discountRate;
            double b = 1 + dr;
            double denom = Math.Pow(b,t);
            double q = timestep * util * (1 /denom);
            return q;
        }

        //****************************************************************************************
        // Computes a transition probability for specified timeStep given a vector containing 
        // two elements - a probability over a duration, and the duration.  
        //****************************************************************************************
        public static double getProb(List<double> a, double timeStep)
        {
            double prob = a[(int) K.ProbTimePair.PROB];
            double years = a[(int) K.ProbTimePair.YEARS];
            double r;
            double p;
            r = Utility.rateFromProb(prob);
            r = r * (1 / years);
            r = r * timeStep;
            p = Utility.probFromRate(r);
            return p;
        }

        //****************************************************************************************
        // Computes a transition probability given:
        // (1) TimeInState in years (double)
        // (2) Timestep in years (double)
        // (3) A list of lists, each of which contains three elements - START year, STOP year, and
        // total probability that event occurs during that period
        //****************************************************************************************
        public static double getProb(double timeInState, double timeStep, List<List<double>> probList)
        {
            double returnVal = 0;

            foreach (List<double> row in probList)
            {
                if (timeInState >= row[(int) K.ProbTimeTriple.START] &&
                    timeInState < row[(int)K.ProbTimeTriple.STOP])
                {
                    List<double> myArray = new List<double>(2);
                    myArray.Add(row[(int)K.ProbTimeTriple.PROB]);
                    myArray.Add(row[(int)K.ProbTimeTriple.STOP] - row[(int)K.ProbTimeTriple.START]);
                    returnVal = Utility.getProb(myArray, K.DELTA_TIME);
                    return returnVal;
                }
            }
            return returnVal;
        }

        //****************************************************************************************
        // Gets background mortality rate as a function of age and the timestep duration, 
        // scaling the corresponding mortality rate by the specified SMR. 
        //****************************************************************************************
        public static double getBackgroundMortProb(double myAge, double timeStep, double SMR)
        {
            double returnVal = 0;
            double mortProbUnadjusted = getBackgroundMortProb(myAge, timeStep);
            double mortRate = SMR * Utility.rateFromProb(mortProbUnadjusted);
            returnVal = Utility.probFromRate(mortRate);
            return returnVal;
        }

        //****************************************************************************************
        // Gets background mortality rate as a function of age and the timestep duration. 
        //****************************************************************************************
        public static double getBackgroundMortProb(double myAge, double timeStep)
        {
            double returnVal = 0;
            int index = (int)myAge;
            index = Math.Min(index, (K.backgroundNeutralDeathProb.Count() - 1));
            double prob = K.backgroundNeutralDeathProb[index];
            if (prob == 1) return prob;
            double r = Utility.rateFromProb(prob);
            r = r * timeStep;
            returnVal = Utility.probFromRate(r);
            return returnVal;
        }
    }
}
