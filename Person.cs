using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyRandom;

namespace HL1
{
    class Person
    {

        // Scenario specifications
        public static K.Tx treatment;
        public static int ageInitial;

        // Acumulator values
        public double lifeYears;
        public double qalyTotal;
        public double ageAtDeath;

        // Status
        public static double timeSinceSimStart;
        public static K.Stage stage;
        public static K.Stage nextStage;
        public static K.LateEffectSeverity lateEffectSeverity;
        public static double timeInCurrentStage;
        public static Boolean alive;
        public static double ageNow;
        public static Boolean firstSimCycle;
        public static K.RelapseHx relapseStatus;

        // Transition probabilities and rates
        static double probToA;
        static double probToB;
        static double probToC1;
        static double probToC2;
        static double probToC3;
        static double probToD;

        static double rateToA;
        static double rateToB;
        static double rateToC1;
        static double rateToC2;
        static double rateToC3;
        static double rateToD;

         // Constructor
        public Person()
        {
        }

        // Set scenario with user selections
        public static void setScenario(String myTx, int myAgeInitial, int myScenario)
        {
            // Set treatment
            if (myTx.ToLower() == K.treatmentNames[(int)K.Tx.CHEMOONLY]) treatment = K.Tx.CHEMOONLY;
            if (myTx.ToLower() == K.treatmentNames[(int)K.Tx.CMT]) treatment = K.Tx.CMT;

            // Set age
            ageInitial = myAgeInitial;

            // Revise default values based on scenario
            K.setScenario(myScenario);

            // Set up transition probability parameters as a function of treatment
            // K.probAtoB[0] = K.probAtoB_Vals[(int)treatment, 0];
            // K.probAtoB[1] = K.probAtoB_Vals[(int)treatment, 1];
            // K.probAtoD_FromTx[0] = K.probAtoD_FromTx[(int)treatment, 0];
            // K.probAtoD_FromTx[1] = K.probAtoD_FromTx_Vals[(int)treatment, 1];
            // K.maxTimeInA = K.maxTimeInA_Vals[(int)treatment, 0];

            /*
            K.probBtoD[0] = K.probBtoD_Vals[(int)treatment, 0];
            K.probBtoD[1] = K.probBtoD_Vals[(int)treatment, 1];
            K.maxTimeInB = K.maxTimeInB_Vals[(int)treatment, 0];
            */

            // K.probC1toC3[0] = K.probC1toC3_Vals[(int)treatment, 0];
            // K.probC1toC3[1] = K.probC1toC3_Vals[(int)treatment, 1];
            // K.latencyTimeInC1 = K.latencyTimeInC1_Vals[(int)treatment, 0];
            // K.probC2toC3[0] = K.probC2toC3_Vals[(int)treatment, 0];
            // K.probC2toC3[1] = K.probC2toC3_Vals[(int)treatment, 1];
            // K.latencyTimeInC2 = K.latencyTimeInC2_Vals[(int)treatment, 0];

        }

        // Get functions
        public int getAgeInitial() { return ageInitial; }
        public Boolean isAlive() { return alive; }
        public double getAgeAtDeath() { return ageAtDeath; }
        public K.Stage getStage() { return stage; }
        public Person getPersonCopy() { Person p = new Person(); return p; }

        //************************************************************************************************
        // Simulation functions
        //************************************************************************************************

        /////////////////////////////////////////////////////////////////////////
        // Top level aging function - call appropriate function based on the current stage
        /////////////////////////////////////////////////////////////////////////
        public void age(double deltaTime)
        {
            switch (stage)
            {
                case K.Stage.A: doStage_A(K.PROBTIMESTEP);  break;
                case K.Stage.B: doStage_B(K.PROBTIMESTEP);  break;
                case K.Stage.C1: doStage_C1(K.PROBTIMESTEP);  break;
                case K.Stage.C2: doStage_C2(K.PROBTIMESTEP);  break;
                case K.Stage.C3: doStage_C3(K.PROBTIMESTEP);  break;
                default: return;
            }

            // Record life years and QALYs for current stage.
            stayInCurrentStage(deltaTime);

            // See if we have to move to a new stage - and move if we the next stage is new.
            nextStage = getNextStage(stage,deltaTime);
            if (nextStage != stage) { moveToNewStage(nextStage); }

        }

        /////////////////////////////////////////////////////////////////////////
        // initializePerson - Sets 
        /////////////////////////////////////////////////////////////////////////
        public void initializePerson()
        {
            // Acumulator values
            lifeYears = 0;
            qalyTotal = 0;
            ageAtDeath = 0;

            // Status values
            timeSinceSimStart = 0;
            stage = K.initStage;
            timeInCurrentStage = 0;
            firstSimCycle = true;
            relapseStatus = K.RelapseHx.NEVER_RELAPSED;

            // Half timestep correction
            age(K.DELTA_TIME * 0.5);

            // Flag that the subject is on the first cycle through the model
            firstSimCycle = false;
        }

        /////////////////////////////////////////////////////////////////////////
        // Ages person by duration in the current stage.
        /////////////////////////////////////////////////////////////////////////
        private void stayInCurrentStage(double duration)
        {
            // Assign "alive" based on the current stage
            alive = (stage != K.deadStage);

            // Identify midpoint of current timestep
            double midpoint = timeSinceSimStart + 0.5 * duration;

            // Increment timeSinceSimStart
            timeSinceSimStart += duration;

            // Increment lifeYears only if still alive
            if (alive)
            {
                lifeYears += duration;
                ageNow = ageInitial + lifeYears;
            }

            // Increment qalyTotal
            double discountFactor = 1/Math.Pow(1 + K.discountRate, midpoint);
            qalyTotal += myUtilityWt() * duration * discountFactor;

            // Time in current state incremented
            timeInCurrentStage += duration;
        }

        /////////////////////////////////////////////////////////////////////////
        // moveToNewStage(K.Stage newStage)
        /////////////////////////////////////////////////////////////////////////
        private void moveToNewStage(K.Stage newStage)
        {
            stage = newStage;                   // Update stage to new stage
            timeInCurrentStage = 0;             // Reset timeInCurrentStage to zero
            alive = (stage != K.deadStage);     // Set alive appropriately
            if (!alive) ageAtDeath = ageNow;    // If subject died, then set ageAtDeath
        }

        /////////////////////////////////////////////////////////////////////////
        // myUtilityWt - returns utility weight for the current stage
        /////////////////////////////////////////////////////////////////////////
        private double myUtilityWt()
        {
            switch (stage)
            {
                case K.Stage.A: return K.utilityA_Vals[0];
                case K.Stage.B: return K.utilityB_Vals[0];
                case K.Stage.C1: return K.utilityC1_Vals[0];
                case K.Stage.C2: return K.utilityC2_Vals[0];
                case K.Stage.C3: return K.utilityC3_Vals[(int)lateEffectSeverity];
                case K.Stage.D: return K.utilityD_Vals[0];
                default: return 0;
            }
        }


        /////////////////////////////////////////////////////////////////////////
        // Zero out transition probabilities and rates before computing their value for the current state
        /////////////////////////////////////////////////////////////////////////
        private void zeroOutProbs()
        {
            probToA = 0; rateToA = 0;
            probToB = 0; rateToB = 0;
            probToC1 = 0; rateToC1 = 0;
            probToC2 = 0; rateToC2 = 0;
            probToC3 = 0; rateToC3 = 0;
            probToD = 0; rateToD = 0;
        }

        /////////////////////////////////////////////////////////////////////////
        // Once the transition probabilities have been computed for the current state, figure out
        // which state will be next and do the transition, as needed.
        /////////////////////////////////////////////////////////////////////////
        private K.Stage getNextStage(K.Stage currentStage, double myTimeStep)
        {
            // If all the transition probabilities are zero, stay in the current stage and return immediately
            if (probToA + probToB + probToC1 + probToC2 + probToC3 + probToD <= K.tinyProb)
                return currentStage;

            // If any of transition probabilities = 1.0, automatically proceed to that state
            if (probToA >= 1) { return K.Stage.A; }
            if (probToB >= 1) { return K.Stage.B; }
            if (probToC1 >= 1) { return K.Stage.C1; }
            if (probToC2 >= 1) { return K.Stage.C2; }
            if (probToC3 >= 1) { return K.Stage.C3; }
            if (probToD >= 1) { return K.Stage.D; }

            // Calculate corresponding rates that match the duration of myTimeStep.  
            rateToA = myTimeStep * Utility.rateFromProb(probToA);
            rateToB = myTimeStep * Utility.rateFromProb(probToB);
            rateToC1 = myTimeStep * Utility.rateFromProb(probToC1);
            rateToC2 = myTimeStep * Utility.rateFromProb(probToC2);
            rateToC3 = myTimeStep * Utility.rateFromProb(probToC3);
            rateToD = myTimeStep * Utility.rateFromProb(probToD);

            // Adjust rates to account for probability time period in K file.  This step inflates the rate up
            // to its value for one year.
            rateToA = rateToA/K.PROBTIMESTEP;
            rateToB = rateToB / K.PROBTIMESTEP;
            rateToC1 = rateToC1 / K.PROBTIMESTEP;
            rateToC2 = rateToC2 / K.PROBTIMESTEP;
            rateToC3 = rateToC3 / K.PROBTIMESTEP;
            rateToD = rateToD / K.PROBTIMESTEP;

            // Convert these rates back to probabilies
            probToA = Utility.probFromRate(rateToA);
            probToB = Utility.probFromRate(rateToB);
            probToC1 = Utility.probFromRate(rateToC1);
            probToC2 = Utility.probFromRate(rateToC2);
            probToC3 = Utility.probFromRate(rateToC3);
            probToD = Utility.probFromRate(rateToD);

            // Finally, set the probability of remaining in the current state equal to 1 minus the probability of
            // proceeding to any of the other states.
            switch (currentStage)
            {
                case K.Stage.A: probToA = 1 - (probToB + probToC1 + probToC2 + probToC3 + probToD); break;
                case K.Stage.B: probToB = 1 - (probToA + probToC1 + probToC2 + probToC3 + probToD); break;
                case K.Stage.C1: probToC1 = 1 - (probToA + probToB + probToC2 + probToC3 + probToD); break;
                case K.Stage.C2: probToC2 = 1 - (probToA + probToB + probToC1 + probToC3 + probToD); break;
                case K.Stage.C3: probToC3 = 1 - (probToA + probToB + probToC1 + probToC2 + probToD); break;
                case K.Stage.D: probToD = 1 - (probToA + probToB + probToC1 + probToC2 + probToC3); break;
                default: probToD = 1 - (probToA + probToB + probToC1 + probToC2 + probToC3); break;
            }

            if (probToA<0 || probToB<0 || probToC1<0 || probToC2<0 || probToC3<0 || probToD<0)
            {
                Console.WriteLine("Probability less than zero error.  Hit any key to exit program.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            double randomNum = MyRandom.RandomProvider.Next();
            K.Stage returnVal = currentStage;
            if (randomNum <= probToA) { returnVal = K.Stage.A; return returnVal; }
            if (randomNum <= probToA + probToB) { returnVal = K.Stage.B; return returnVal; }
            if (randomNum <= probToA + probToB + probToC1) { returnVal = K.Stage.C1; return returnVal; }
            if (randomNum <= probToA + probToB + probToC1 + probToC2) { returnVal = K.Stage.C2; return returnVal; }
            if (randomNum <= probToA + probToB + probToC1 + probToC2 + probToC3) { returnVal = K.Stage.C3; return returnVal; }
            if (randomNum <= probToA + probToB + probToC1 + probToC2 + probToC3 + probToD) { returnVal = K.Stage.D; return returnVal; }
            return returnVal;
        }


        /////////////////////////////////////////////////////////////////////////
        // doStage_A - At risk state
        /////////////////////////////////////////////////////////////////////////
        public void doStage_A(double deltaTime)
        {

            // Initialize transition probabilities and rates to zero
            zeroOutProbs();

            // If limit reached for time in at-risk state (A) reached, then go to cured state w/o relapse (C2)
            if (timeInCurrentStage + deltaTime >= K.maxTimeInA[(int)treatment].ElementAt(0)) probToC2 = 1;

            // If limit for time in at-risk state not reached, then figure out if we go to relapse (B) or dead (D)
            if (timeInCurrentStage + deltaTime < K.maxTimeInA[(int)treatment].ElementAt(0))
            {
                probToB = Utility.getProb(K.probAtoB[(int)treatment], deltaTime);
                probToC2 = 0;
                probToD = MortProb_StageA(deltaTime);
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // MortProb_StageA
        // Sums mortality rate contributions from background and from treatment
        // to come up with a total mortality rate.
        /////////////////////////////////////////////////////////////////////////
        double MortProb_StageA(double deltaTime)
        {
            double MortProb_Background_PerStep = Utility.getBackgroundMortProb(ageNow, deltaTime);
            double MortRate_Background_PerStep = Utility.rateFromProb(MortProb_Background_PerStep);

            double MortProb_Tx_PerStep = Utility.getProb(K.probAtoD_FromTx[(int)treatment], deltaTime);
            double MortRate_Tx_PerStep = Utility.rateFromProb(MortProb_Tx_PerStep);

            double MortRate_Total_PerStep = MortRate_Background_PerStep + MortRate_Tx_PerStep;
            double MortProb_Total_PerStep = Utility.probFromRate(MortRate_Total_PerStep);

            return MortProb_Total_PerStep;
        }

        /////////////////////////////////////////////////////////////////////////
        // doStage_B - relapse state
        /////////////////////////////////////////////////////////////////////////
        public void doStage_B(double deltaTime)
        {
            // Initialize transition probabilities and rates to zero
            zeroOutProbs();

            // Set haveRelapsed flag to TRUE
            relapseStatus = K.RelapseHx.HAVE_RELAPSED;

            // If limit reached for time in relapse state (B) reached, then go to cured state (C1)
            if (timeInCurrentStage + deltaTime >= K.maxTimeInB[(int) treatment].ElementAt(0)) probToC1 = 1;

            // If limit for time in relapse state (B) not reached, then there is no possibility of cure (C1).  So
            // figure out if the subject dies - i.e., goes to (D)
            if (timeInCurrentStage + deltaTime < K.maxTimeInB[(int)treatment].ElementAt(0))
            {
                probToC1 = 0;

                foreach (List<double> row in K.probBtoD[(int)treatment])
                {
                    if (timeInCurrentStage >= row[(int)K.probBtoD_dims.START] &&
                        timeInCurrentStage < row[(int)K.probBtoD_dims.STOP])
                    {
                        List<double> myArray = new List<double>(2);
                        for (int i = 0; i < 2; i++) myArray.Add(0);
                        myArray[(int) K.ProbTimePair.PROB] = row[(int)K.probBtoD_dims.PROB];
                        myArray[(int) K.ProbTimePair.YEARS] = row[(int)K.probBtoD_dims.STOP] - row[(int)K.probBtoD_dims.START];
                        probToD = Utility.getProb(myArray, deltaTime);
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // doStage_C1 - cured state with relapse
        /////////////////////////////////////////////////////////////////////////
        public void doStage_C1(double deltaTime)
        {
            // Initialize transition probabilities and rates to zero
            zeroOutProbs();

            // Get mortality probability
            probToD = Utility.getBackgroundMortProb(ageNow, deltaTime);

            // Get transition probability to C3 based on time since entering C1
            probToC3 = Utility.getProb(timeInCurrentStage, deltaTime, K.probC1toC3[(int)treatment]);
        }

        /////////////////////////////////////////////////////////////////////////
        // doStage_C2 - cured state without relapse
        /////////////////////////////////////////////////////////////////////////
        public void doStage_C2(double deltaTime)
        {
            // Initialize transition probabilities and rates to zero
            zeroOutProbs();

            // Get mortality probability
            probToD = Utility.getBackgroundMortProb(ageNow, deltaTime);

            // Get transition probability to C3 based on time since entering C1
            probToC3 = Utility.getProb(timeInCurrentStage, deltaTime, K.probC2toC3[(int)treatment]);
        }

        /////////////////////////////////////////////////////////////////////////
        // doStage_C3 - cured state with late effects
        /////////////////////////////////////////////////////////////////////////
        public void doStage_C3(double deltaTime)
        {
            // Initialize transition probabilities and rates to zero
            zeroOutProbs();

            // If the subject just arrived in this stage, then set the severity level.
            if (timeInCurrentStage == 0) setLateEffectSeverity();

            // Get mortality probability
            probToD = Utility.getBackgroundMortProb(ageNow, deltaTime, getC3_SMR());
        }

        private double getC3_SMR()
        {
            // if (lateEffectSeverity == K.LateEffectSeverity.SEVERE && timeInCurrentStage>15) { int i = 1; }

            double returnVal = 1.0;
            foreach (List<double> segment in K.probC3toD_SMR[lateEffectSeverity])
            {
                if (timeInCurrentStage >= segment[(int) K.probC3toD_SMR_offset.STARTYR] &&
                    timeInCurrentStage < segment[(int) K.probC3toD_SMR_offset.ENDYR])
                {
                    double startYr = segment[(int)K.probC3toD_SMR_offset.STARTYR];
                    double endYr = segment[(int)K.probC3toD_SMR_offset.ENDYR];
                    double startSMR = segment[(int)K.probC3toD_SMR_offset.STARTSMR];
                    double endSMR = segment[(int)K.probC3toD_SMR_offset.ENDSMR];
                    double segmentFraction = (timeInCurrentStage - startYr) / (endYr - startYr);
                    returnVal = startSMR + segmentFraction * (endSMR - startSMR);
                    return returnVal;
                }
            }
            return returnVal;
        }

        /////////////////////////////////////////////////////////////////////////
        // getLateEffectSeverity - returns late effect severity - chosen 
        // randomly with probability dependent on (a) whether patient has relapsed
        // and (b) type of treatment received.
        /////////////////////////////////////////////////////////////////////////
        void setLateEffectSeverity()
        {
            float myProb = (float) K.probSeverityC3_Vals[(int)treatment, (int)relapseStatus];
            if (RandomProvider.NextBool(myProb))
                lateEffectSeverity = K.LateEffectSeverity.SEVERE;
            else
                lateEffectSeverity = K.LateEffectSeverity.MILD;
        }
    }
}

