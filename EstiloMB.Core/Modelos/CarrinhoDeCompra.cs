using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EstiloMB.Core.Modelos
{
    public class CarrinhoDeCompra
    {
        public int Id { get; set; }
        public List<ItemPedido> Itens { get; set; }

        public CarrinhoDeCompra()
        {
            Itens = new List<ItemPedido>();
        }

        public void AdicionarItem(ItemPedido itemPedido)
        {
            // Verifica se o item já existe no carrinho
            var itemExistente = Itens.FirstOrDefault(i => i.ProdutoID == itemPedido.ProdutoID && i.Tamanho == itemPedido.Tamanho && i.CorID == itemPedido.CorID);
            if (itemExistente != null)
            {
                // Se o item já existe, aumenta a quantidade em 1
                itemExistente.Quantidade++;
            }
            else
            {
                // Se o item não existe, adiciona ao carrinho
                Itens.Add(itemPedido);
            }
        }


    }
}
