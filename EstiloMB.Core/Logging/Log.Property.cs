using System;

namespace Chargeback.Core
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class LogProperty : Attribute
    {

    }
}