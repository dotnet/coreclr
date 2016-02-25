using System;

public class Test
{
    public static int Main(string[] args)
    {
        string test = null;
        
        try
        {
            try
            {
                Console.WriteLine("Hello {0}", test.Length);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Pass");
                return 100;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAIL");
            return 101;
        }

        Console.WriteLine("FAIL");
        return 102;
    }
}
