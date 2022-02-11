using NLog;

var logger = LogManager.GetLogger("DemoLogger");

logger.Info("Hello world!");

try
{
    ThrowAnException();
}
catch (Exception ex)
{
    logger.Error(ex, "Failure message.");
}


void ThrowAnException()
{
    throw new ArgumentOutOfRangeException("someArgumentName");
}