using CoreWCF;

namespace SoapServiceDemo.Services;

[ServiceContract]
public interface ICalculatorService
{
    [OperationContract]
    int Add(int a, int b);
}