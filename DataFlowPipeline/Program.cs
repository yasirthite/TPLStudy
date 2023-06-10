
using DataFlowPipeline;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
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

        Parallel.ForEach(uris, item => new DataProcessFlow(timer, item).Start());

        //DataProcessFlow[] dataProcessFlows = new DataProcessFlow[uris.Length];
        //int i = 0;
        //foreach (var uri in uris)
        //{
        //    //downloadString.Post(uri);
        //    dataProcessFlows[i] = new DataProcessFlow(timer, uri);
        //    dataProcessFlows[i].Start();
        //    i++;
        //}

        //dataProcessFlows.WaitForAll();

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