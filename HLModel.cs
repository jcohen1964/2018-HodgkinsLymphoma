using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL1
{
    class HLModel
    {

        // Person template for cohort
        Person simPerson;

        // Pointer to list of simulated cohort
        Person[] personArray = new Person[K.NUM_PERSONS];

        // List of statistics to report
        LinkedList<string> outputStats = new LinkedList<String>();

        // String containing the simulation results;
        String simOutputString = new string(' ', 0);

        public HLModel(Person person)
        {

            //*************************************************************************************
            // Initialization
            // Apparently, at least one instance of the ProbabilityDeath object must be created 
            // in order to get the arrays in the class definition created.  That is what the foo
            // instance is.  It is not actually used for anything.
            //*************************************************************************************
            //Parms foo = new Parms();

            // Point the simPerson to the person passed to the model
            simPerson = person;

            // set up list of statistics to report out for each outcome
            setUpStatList();
        }

        //****************************************************************************************
        // runSimulation()
        //****************************************************************************************
        public void runSimulation()
        {
            // Create an array of subjects.
            for (int i = 0; i < K.NUM_PERSONS; i++)
            {
                personArray[i] = simPerson.getPersonCopy();
            }

            // Send each one through the simulation.
            for (int j = 0; j < personArray.GetLength(0); j++)
            {
                (personArray[j]).initializePerson();
                while (personArray[j].isAlive()) (personArray[j]).age(K.DELTA_TIME);
            }
        }


        //****************************************************************************************
        // Creates a linked list of statistics to report out for each outcome variable
        //****************************************************************************************
        void setUpStatList()
        {
            outputStats.AddLast("mean");
            outputStats.AddLast("variance");
            outputStats.AddLast("min");
            outputStats.AddLast("max");
            outputStats.AddLast("pctl|0.05");
        }

        public void reportAllStats(string title)
        {
            simOutputString = title + "\n\n";

            // Set up linked list to hold the values from one quantity for which we will
            // compute summary statistics.
            List<Double> tempValues = new List<Double>();

            // Get summary stats for life years.
            tempValues.Clear();
            for (int j = 0; j < personArray.GetLength(0); j++) { tempValues.Add((personArray[j]).lifeYears); }
            simOutputString = simOutputString + "LIFE YEARS LIVED FOLLOWING DIAGNOSIS\n";
            simOutputString = simOutputString + reportStatsOneQuantity(tempValues) + "\n";

            // Get summary stats for age at death.
            tempValues.Clear();
            for (int j = 0; j < personArray.GetLength(0); j++) { tempValues.Add((personArray[j]).ageAtDeath); }
            simOutputString = simOutputString + "AGE AT DEATH\n";
            simOutputString = simOutputString + reportStatsOneQuantity(tempValues) + "\n";

            //get summary stats for qaly 
            tempValues.Clear();
            for (int j = 0; j < personArray.GetLength(0); j++) { tempValues.Add((personArray[j]).qalyTotal); }
            simOutputString = simOutputString + "QALY\n";
            simOutputString = simOutputString + reportStatsOneQuantity(tempValues) + "\n";

        }

        public void PauseModelOutput(bool waitForKey)
        { 
            Console.WriteLine(simOutputString);
            if (waitForKey)
            {
                Console.WriteLine("Hit any key to exit\n\n");
                Console.ReadKey();
            }
        }

        String reportStatsOneQuantity(List<Double> values)
        {
            String returnString = "";
            double resultHolder;
            double mean = 0;
            values.Sort();

            // Compute mean - may be used for computation of variance
            double sum = 0;
            foreach (double d in values) { sum += d; }
            mean = sum / values.Count();

            foreach (string stat in outputStats)
            {

                // Output mean if it that stat is requested
                if (stat.Equals("mean"))
                {
                    returnString = returnString + "mean:\t" + mean.ToString() + "\n";
                }

                // Output variance if that stat is requested
                if (stat.Equals("variance"))
                {
                    sum = 0;
                    foreach (double d in values) { sum += (mean - d) * (mean - d); }
                    resultHolder = sum / values.Count();
                    returnString = returnString + "variance:\t" + resultHolder.ToString() + "\n";
                }

                // Output min and max if they are requested
                if (stat.Equals("min"))
                {
                    resultHolder = values[0];
                    returnString = returnString + "minimum:\t" + resultHolder.ToString() + "\n";
                }
                if (stat.Equals("max"))
                {
                    resultHolder = values[values.Count() - 1];
                    returnString = returnString + "maximum:\t" + resultHolder.ToString() + "\n";
                }

            }
            return returnString;
        }
    }
}
