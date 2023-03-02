using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EstiloMB.Core
{
    public class Correios
    {
        private readonly HttpClient _httpClient;

        public Correios()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://viacep.com.br/") };
        }

        public async Task<Correios> ConsultarCepAsync(string cep)
        {
            try
            {
                var response = await _httpClient.GetAsync($"ws/{cep}/json/");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Correios>(result);
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Falha na chamada à API dos Correios", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Falha na desserialização do JSON retornado pela API dos Correios", ex);
            }
        }

        private const string CorreiosApiUrl = "http://ws.correios.com.br/";

        public async Task<Correios> ConsultarPrazoAsync(/*string codigoServico,*/ string cepOrigem, string cepDestino)
        {
            //if (string.IsNullOrEmpty(codigoServico))
            //{
            //    throw new ArgumentException("O código de serviço é obrigatório.", nameof(codigoServico));
            //}

            if (string.IsNullOrEmpty(cepOrigem))
            {
                throw new ArgumentException("O CEP de origem é obrigatório.", nameof(cepOrigem));
            }

            if (string.IsNullOrEmpty(cepDestino))
            {
                throw new ArgumentException("O CEP de destino é obrigatório.", nameof(cepDestino));
            }

            try
            {
                //var requestUrl = $"{CorreiosApiUrl}calculador/CalcPrecoPrazo.aspx?nCdServico={codigoServico}&sCepOrigem={cepOrigem}&sCepDestino={cepDestino}&nVlPeso=1&nCdFormato=1&nVlComprimento=20&nVlAltura=20&nVlLargura=20&nVlDiametro=0&sCdMaoPropria=n&sCdAvisoRecebimento=n&sCdValorDeclarado=0";
                var requestUrl = $"{CorreiosApiUrl}calculador/CalcPrecoPrazo.aspx?sCepOrigem={cepOrigem}&sCepDestino={cepDestino}&nVlPeso=1&nCdFormato=1&nVlComprimento=20&nVlAltura=20&nVlLargura=20&nVlDiametro=0&sCdMaoPropria=n&sCdAvisoRecebimento=n&sCdValorDeclarado=0";

                using (var response = await _httpClient.GetAsync(requestUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<Correios>(result);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Falha na chamada à API dos Correios", ex);
            }
        }

    }

}
