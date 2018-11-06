using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Instance
{
    public interface IBaseService
    {
        TimeSpan Schedule { get; }
        Action Fire { get; }
    }
}
