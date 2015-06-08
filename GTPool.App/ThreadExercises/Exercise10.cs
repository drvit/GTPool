using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.App.ThreadExercises
{
    public class Exercise10
    {
        static readonly object FinishedLock = new object();
        const string PageUrl = @"http://www.pobox.com/~skeet/csharp/threads/threadpool.shtml";
    
        public static void Run()
        {
            var request = WebRequest.Create(PageUrl);
            var state = new RequestResponseState {Request = request};

            // Lock the object we'll use for waiting now, to make
            // sure we don't (by some fluke) do everything in the other threads
            // before getting to Monitor.Wait in this one. If we did, the pulse
            // would effectively get lost!
            lock (FinishedLock)
            {
                request.BeginGetResponse(GetResponseCallback, state);

                Console.WriteLine("Waiting for response...");

                // Wait until everything's finished. Normally you'd want to
                // carry on doing stuff here, of course.
                Monitor.Wait(FinishedLock);
            }
        }

        static void GetResponseCallback(IAsyncResult ar)
        {
            // Fetch our state information
            var state = (RequestResponseState)ar.AsyncState;

            // Fetch the response which has been generated
            state.Response = state.Request.EndGetResponse(ar);

            // Store the response stream in the state
            state.Stream = state.Response.GetResponseStream();

            // Stash an Encoding for the text. I happen to know that
            // my web server returns text in ISO-8859-1 - which is
            // handy, as we don't need to worry about getting half
            // a character in one read and the other half in another.
            // (Use a Decoder if you want to cope with that.)
            state.Encoding = Encoding.GetEncoding(28591);

            // Now start reading from it asynchronously
            if (state.Stream != null)
                state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                    ReadCallback, state);
        }

        static void ReadCallback(IAsyncResult ar)
        {
            // Fetch our state information
            var state = (RequestResponseState)ar.AsyncState;

            // Find out how much we've read
            var len = state.Stream.EndRead(ar);

            // Have we finished now?
            if (len == 0)
            {
                // Dispose of things we can get rid of
                ((IDisposable)state.Response).Dispose();
                ((IDisposable)state.Stream).Dispose();
                ReportFinished(state.Text.ToString());
                return;
            }

            // Nope - so decode the text and then call BeginRead again
            state.Text.Append(state.Encoding.GetString(state.Buffer, 0, len));

            state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length,
                                   ReadCallback, state);
        }

        static void ReportFinished(string page)
        {
            Console.WriteLine("Read text of page. Length={0} characters.", page.Length);
            // Assume for convenience that the page length is over 50 characters!
            Console.WriteLine("First 50 characters:");
            Console.WriteLine(page.Substring(0, 50));
            Console.WriteLine("Last 50 characters:");
            Console.WriteLine(page.Substring(page.Length - 50));

            // Tell the main thread we've finished.
            lock (FinishedLock)
            {
                Monitor.Pulse(FinishedLock);
            }
        }

        class RequestResponseState
        {
            // In production code, you may well want to make these properties,
            // particularly if it's not a private class as it is in this case.
            internal WebRequest Request;
            internal WebResponse Response;
            internal Stream Stream;
            internal readonly byte[] Buffer = new byte[16384];
            internal Encoding Encoding;
            internal readonly StringBuilder Text = new StringBuilder();
        }
    }
}
