using System.Text;
using Heartbeat.Runtime.Analyzers.Interfaces;
using Heartbeat.Runtime.Proxies;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Runtime.Analyzers
{
    public sealed class HttpWebResponseAnalyzer : ProxyInstanceAnalyzerBase<HttpWebResponseProxy>, ILoggerDump
    {
        public HttpWebResponseAnalyzer(RuntimeContext context, HttpWebResponseProxy targetObject)
            : base(context, targetObject)
        {
        }

        public void Dump(ILogger logger)
        {
            var responseProxy = TargetObject;

            logger.LogInformation(responseProxy.TargetObject.ToString());
            logger.LogInformation($"Status: {responseProxy.StatusCode} {responseProxy.StatusDescription}");
            logger.LogInformation($"ContentLength: {responseProxy.ContentLength}");

            var connectStreamObject = responseProxy.TargetObject.GetObjectField("m_ConnectStream");
            int doneCalled = -1;

            if (!connectStreamObject.IsNull)
            {
                doneCalled = connectStreamObject.GetField<int>("m_DoneCalled");

                var readBuffer = connectStreamObject.GetObjectField("m_ReadBuffer");
                var readBufferLength = readBuffer.Type.GetArrayLength(readBuffer.Address);
                var startDataAddress = readBuffer.Type.GetArrayElementAddress(readBuffer.Address, 0);

                var buffer = new byte[readBufferLength];

                Context.Heap.ReadMemory(startDataAddress, buffer, 0, readBufferLength);

                var readBufferSize = connectStreamObject.GetField<int>("m_ReadBufferSize");
                var readOffset = connectStreamObject.GetField<int>("m_ReadOffset");

                var s = Encoding.UTF8.GetString(buffer, readOffset, readBufferSize);

                logger.LogInformation($"m_ConnectStream.m_ReadBuffer: {s}");
            }
            logger.LogInformation($"m_ConnectStream.m_DoneCalled: {doneCalled}");

            var headerCollectionAnalyzer = new WebHeaderCollectionAnalyzer(Context, responseProxy.Headers);
            using (logger.BeginScope("Headers"))
            {
                headerCollectionAnalyzer.Dump(logger);
            }
        }
    }
}