using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.Common.Exceptions
{
    public class NotFoundException(string message) : Exception(message);

}