using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace DataFlowPipeline
{
    internal class DataProcessFlow
    {
        private readonly Stopwatch globalTimer;
        private readonly Stopwatch localTimer;
        private readonly string Uri;
        private TransformBlock<string, string> downloadString;
        private ActionBlock<string> printReversedWords;
        public DataProcessFlow(Stopwatch globalTimer, string uri)
        {
            this.globalTimer = globalTimer;
            this.Uri = uri;
            localTimer = new Stopwatch();
        }

        public void Start()
        {
            localTimer.Start();

            TransformBlock<string, string> downloadString = GetDownloadStringDataBlock();
            this.downloadString = downloadString;
            TransformBlock<string, string[]> createWordList = GetCreateWordListDataBlock();
            TransformBlock<string[], string[]> filterWordList = GetFilterWordListDataBlock();
            TransformManyBlock<string[], string> findReversedWords = GetFindReverseWordsDataBlock();
            ActionBlock<string> printReversedWords = GetPrintReverseWordsDataBlock();
            this.printReversedWords = printReversedWords;

            CreateDataFlowPipeline(downloadString, createWordList, filterWordList, findReversedWords, printReversedWords);

            TriggerPipelineExecution(downloadString, printReversedWords);

            //Console.WriteLine($"Global Elapsed Time: {globalTimer.Elapsed.Seconds} sec(s) \n" +
            //    $"Local Elapsed Time: {localTimer.Elapsed.Seconds} sec(s)" +
            //    $"For Uri: {this.Uri}");

            Console.WriteLine($"Local Elapsed Time: {localTimer.Elapsed.Seconds} sec(s)" +
                $"For Uri: {this.Uri}");

            localTimer.Stop();
        }
        private void CreateDataFlowPipeline(TransformBlock<string, string> downloadString, TransformBlock<string, string[]> createWordList, TransformBlock<string[], string[]> filterWordList, TransformManyBlock<string[], string> findReversedWords, ActionBlock<string> printReversedWords)
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            downloadString.LinkTo(createWordList, linkOptions);
            createWordList.LinkTo(filterWordList, linkOptions);
            filterWordList.LinkTo(findReversedWords, linkOptions);
            findReversedWords.LinkTo(printReversedWords, linkOptions);
        }

        private void TriggerPipelineExecution(TransformBlock<string, string> downloadString, ActionBlock<string> printReversedWords)
        {
            //The following SendAsync Method triggers data flow.
            downloadString.SendAsync(this.Uri);
            this.downloadString.Complete();
            //Wait(downloadString, printReversedWords);
        }

        public void Wait()
        {
            //this.downloadString.Complete();
            this.printReversedWords.Completion.Wait();
        }

        private static ActionBlock<string> GetPrintReverseWordsDataBlock()
        {
            return new ActionBlock<string>(reversedWord =>
            {
                Console.WriteLine("Found reversed words {0}/{1}",
                   reversedWord, new string(reversedWord.Reverse().ToArray()));
            });
        }

        private static TransformManyBlock<string[], string> GetFindReverseWordsDataBlock()
        {
            return new TransformManyBlock<string[], string>(words =>
            {
                Console.WriteLine("Finding reversed words...");

                var wordsSet = new HashSet<string>(words);

                return from word in words.AsParallel()
                       let reverse = new string(word.Reverse().ToArray())
                       where word != reverse && wordsSet.Contains(reverse)
                       select word;
            });
        }

        private static TransformBlock<string[], string[]> GetFilterWordListDataBlock()
        {
            return new TransformBlock<string[], string[]>(words =>
            {
                Console.WriteLine("Filtering word list...");

                return words
                   .Where(word => word.Length > 3)
                   .Distinct()
                   .ToArray();
            });
        }

        private static TransformBlock<string, string[]> GetCreateWordListDataBlock()
        {
            return new TransformBlock<string, string[]>(text =>
            {
                Console.WriteLine("Creating word list...");

                char[] tokens = text.Select(c => char.IsLetter(c) ? c : ' ').ToArray();
                text = new string(tokens);

                return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            });
        }

        private TransformBlock<string, string> GetDownloadStringDataBlock()
        {
            return new TransformBlock<string, string>(async uri =>
            {
                //Console.WriteLine($"Started Downloading [{uri}] \n Global Time Ellapsed: {globalTimer.ElapsedMilliseconds}");
                var data = await new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }).GetStringAsync(uri = this.Uri);
                //Console.WriteLine($"Finished Downloading [{uri}] \n Global Time Ellapsed: {globalTimer.ElapsedMilliseconds}");
                return data;
            });
        }
    }
}
