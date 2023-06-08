/* using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

static partial class Program
{
    static void Main()
    {
        // start stopwatch
        var globalTimer = Stopwatch.StartNew();

        var downloadString = new TransformBlock<string, string>(async uri =>
        {
            Console.WriteLine($"Started Downloading {uri}" +
                $"{Environment.NewLine} {globalTimer.ElapsedMilliseconds}");

            var downloadedData = await new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }).GetStringAsync(uri);

            Console.WriteLine($"Completed Downloading {uri}" +
                $"{Environment.NewLine} {globalTimer.ElapsedMilliseconds}");

            return downloadedData;
        });

        var createWordList = new TransformBlock<string, string[]>(text =>
        {
            Console.WriteLine("Creating word list...");

            // Remove common punctuation by replacing all non-letter characters
            // with a space character.
            char[] tokens = text.Select(c => char.IsLetter(c) ? c : ' ').ToArray();
            text = new string(tokens);

            // Separate the text into an array of words.
            return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        });
        var filterWordList = new TransformBlock<string[], string[]>(words =>
        {
            Console.WriteLine("Filtering word list...");

            return words
               .Where(word => word.Length > 3)
               .Distinct()
               .ToArray();
        });
        var findReversedWords = new TransformManyBlock<string[], string>(words =>
        {
            Console.WriteLine("Finding reversed words...");

            var wordsSet = new HashSet<string>(words);

            return from word in words.AsParallel()
                   let reverse = new string(word.Reverse().ToArray())
                   where word != reverse && wordsSet.Contains(reverse)
                   select word;
        });
        var printReversedWords = new ActionBlock<string>(reversedWord =>
        {
            Console.WriteLine("Found reversed words {0}/{1}",
               reversedWord, new string(reversedWord.Reverse().ToArray()));
        });

        // Form a pipeline.
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        downloadString.LinkTo(createWordList, linkOptions);
        createWordList.LinkTo(filterWordList, linkOptions);
        filterWordList.LinkTo(findReversedWords, linkOptions);
        findReversedWords.LinkTo(printReversedWords, linkOptions);

        // Start both downloads simultaneously
        var downloadTasks = new List<Task>
        {
            downloadString.SendAsync("http://www.gutenberg.org/cache/epub/16452/pg16452.txt"),
            downloadString.SendAsync("https://www.gutenberg.org/cache/epub/70922/pg70922.txt")
        };

        // Wait for both downloads to complete
        Task.WhenAll(downloadTasks).Wait();

        // Mark the head of the pipeline as complete.
        downloadString.Complete();
        //downloadString.Completion.Wait();

        // Wait for the last block in the pipeline to process all messages.
        printReversedWords.Completion.Wait();

        //stop globalTimer and print elapsed time in milliseconds
        globalTimer.Stop();
        Console.WriteLine($"Elapsed Time: {globalTimer.Elapsed.TotalSeconds} Sec(s)");
    }
} 
 */

using DataFlowPipeline;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

static class DataflowReversedWords
{
    public static void Main()
    {
        //start stopwatch
        var timer = Stopwatch.StartNew();

        //TransformBlock<string, string> downloadString = OldCode(timer);

        string[] uris = {
            "http://www.gutenberg.org/cache/epub/16452/pg16452.txt",
            "https://www.gutenberg.org/cache/epub/70922/pg70922.txt",
            "https://www.gutenberg.org/cache/epub/70925/pg70925.txt"};

        //Parallel.ForEach(uris, item => downloadString.SendAsync(item));

        DataProcessFlow[] dataProcessFlows = new DataProcessFlow[uris.Length];
        int i = 0;
        foreach (var uri in uris)
        {
            //downloadString.Post(uri);
            dataProcessFlows[i] = new DataProcessFlow(timer, uri);
            dataProcessFlows[i].Start();
            i++;
        }

        dataProcessFlows.WaitForAll();

        Console.WriteLine($"Total Elapsed Time For All Processes: {timer.Elapsed.Seconds} sec(s).");
    }

    private static TransformBlock<string, string> OldCode(Stopwatch timer)
    {
        var downloadString = new TransformBlock<string, string>(async uri =>
        {
            Console.WriteLine($"Started Downloading [{uri}] \n {timer.ElapsedMilliseconds}");
            var data = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }).GetStringAsync(uri).Result;
            Console.WriteLine($"Finished Downloading [{uri}] \n {timer.ElapsedMilliseconds}");
            return data;
        });

        var createWordList = new TransformBlock<string, string[]>(text =>
        {
            Console.WriteLine("Creating word list...");

            char[] tokens = text.Select(c => char.IsLetter(c) ? c : ' ').ToArray();
            text = new string(tokens);

            return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        });

        var filterWordList = new TransformBlock<string[], string[]>(words =>
        {
            Console.WriteLine("Filtering word list...");

            return words
               .Where(word => word.Length > 3)
               .Distinct()
               .ToArray();
        });

        var findReversedWords = new TransformManyBlock<string[], string>(words =>
        {
            Console.WriteLine("Finding reversed words...");

            var wordsSet = new HashSet<string>(words);

            return from word in words.AsParallel()
                   let reverse = new string(word.Reverse().ToArray())
                   where word != reverse && wordsSet.Contains(reverse)
                   select word;
        });

        var printReversedWords = new ActionBlock<string>(reversedWord =>
        {
            Console.WriteLine("Found reversed words {0}/{1}",
               reversedWord, new string(reversedWord.Reverse().ToArray()));
        });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        downloadString.LinkTo(createWordList, linkOptions);
        createWordList.LinkTo(filterWordList, linkOptions);
        filterWordList.LinkTo(findReversedWords, linkOptions);
        findReversedWords.LinkTo(printReversedWords, linkOptions);
        return downloadString;
    }
}