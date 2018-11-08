using System;
using System.Threading.Tasks;

namespace Service.Instance
{
    public interface IBaseService
    {
        TimeSpan Schedule { get; }
    }
}
