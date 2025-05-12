using System.Collections;

namespace AlfaCRM.Services.Extensions;

public class Helper
{
    public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
    {
        var rng = new Random();
        var elements = source.ToList();

        for (int i = elements.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            (elements[i], elements[swapIndex]) = (elements[swapIndex], elements[i]);
        }

        return elements;
    }

}