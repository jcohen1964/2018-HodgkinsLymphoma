using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL1
{
    class Program
    {
        static void Main(string[] args)
        {
            int MyScenario = 20;
            
            Person person1 = new Person();
            Person.setScenario("chemoOnly", 65, MyScenario); // Scenario numbered 0
            HLModel myModel1 = new HLModel(person1);
            myModel1.runSimulation();
            myModel1.reportAllStats("Chemo Alone");
            myModel1.PauseModelOutput(false);

            Person person2 = new Person();
            Person.setScenario("CMT", 65, MyScenario); // Scenario numbered 0
            HLModel myModel2 = new HLModel(person2);
            myModel2.runSimulation();
            myModel2.reportAllStats("CMT");
            myModel2.PauseModelOutput(true);
        }
    }
}

