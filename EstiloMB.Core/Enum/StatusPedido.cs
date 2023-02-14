using System;
using System.Collections.Generic;
using System.Text;

namespace EstiloMB.Core
{
    public enum StatusPedido
    {
        EmAndamento = 0,
        AguardandoPagamento = 1,
        Enviado = 2,
        Entregue = 3,
        DevolvidoRemetente = 4,
        Postado = 5,
    }
}
