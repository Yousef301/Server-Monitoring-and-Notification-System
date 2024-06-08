namespace MessageProcessingAndAnomalyDetection.ConsoleIO.ConsoleOutput;

public class ConsoleOutput
{
    public static void MessageDisplay(string message, bool newLine = true)
    {
        if (newLine)
        {
            Console.WriteLine(message);
        }
        else
        {
            Console.Write(message);
        }
    }
}