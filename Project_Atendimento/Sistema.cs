using System;
using System.Collections.Generic;
using System.IO;

namespace Project_Atendimento
{
    // ========== ENUMS E STRUCTS ==========
    public enum StatusChamado
    {
        ABERTO,
        EM_ATENDIMENTO,
        RESOLVIDO,
        CANCELADO
    }

    public struct PriorityStruct
    {
        public int Nivel { get; set; }
        public string Descricao { get; set; }

        public PriorityStruct(int nivel, string descricao)
        {
            Nivel = nivel;
            Descricao = descricao;
        }

        public static PriorityStruct Baixa => new PriorityStruct(1, "Baixa");
        public static PriorityStruct Media => new PriorityStruct(2, "Média");
        public static PriorityStruct Alta => new PriorityStruct(3, "Alta");
        public static PriorityStruct Critica => new PriorityStruct(4, "Crítica");

        public override string ToString()
        {
            return $"{Nivel}-{Descricao}";
        }
    }

    // ========== MODELOS ==========
    public class Cliente
    {
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }

        public Cliente()
        {
        }

        public Cliente(int codigo, string nome, string telefone, string email)
        {
            Codigo = codigo;
            Nome = nome;
            Telefone = telefone;
            Email = email;
        }

        public override string ToString()
        {
            return $"Código: {Codigo}, Nome: {Nome}, Telefone: {Telefone}, Email: {Email}";
        }

        public bool IsValid()
        {
            return Codigo > 0 &&
                   !string.IsNullOrWhiteSpace(Nome) &&
                   !string.IsNullOrWhiteSpace(Telefone) &&
                   !string.IsNullOrWhiteSpace(Email);
        }
    }

    public class Chamado
    {
        public int Numero { get; set; }
        public int CodigoCliente { get; set; }
        public string Descricao { get; set; }
        public PriorityStruct Prioridade { get; set; }
        public StatusChamado Status { get; set; }

        public Chamado()
        {
            Status = StatusChamado.ABERTO;
        }

        public Chamado(int numero, int codigoCliente, string descricao, PriorityStruct prioridade)
        {
            Numero = numero;
            CodigoCliente = codigoCliente;
            Descricao = descricao;
            Prioridade = prioridade;
            Status = StatusChamado.ABERTO;
        }

        public override string ToString()
        {
            return $"Número: {Numero}, Cliente: {CodigoCliente}, Descrição: {Descricao}, Prioridade: {Prioridade}, Status: {Status}";
        }

        public bool IsValid()
        {
            return Numero > 0 &&
                   CodigoCliente > 0 &&
                   !string.IsNullOrWhiteSpace(Descricao);
        }
    }

    // ========== SERVIÇO DE CLIENTE ==========
    public class ClienteService
    {
        private List<Cliente> clientes = new List<Cliente>();

        public void CadastrarCliente(int codigo, string nome, string telefone, string email)
        {
            if (codigo <= 0)
                throw new ArgumentException("Código deve ser maior que zero");

            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome não pode estar vazio");

            if (string.IsNullOrWhiteSpace(telefone))
                throw new ArgumentException("Telefone não pode estar vazio");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode estar vazio");

            if (ExisteClienteComCodigo(codigo))
                throw new ArgumentException("Cliente com este código já existe");

            Cliente cliente = new Cliente(codigo, nome, telefone, email);
            clientes.Add(cliente);
        }

        public Cliente ObterClientePorCodigo(int codigo)
        {
            foreach (Cliente cliente in clientes)
            {
                if (cliente.Codigo == codigo)
                    return cliente;
            }
            return null;
        }

        public bool ExisteClienteComCodigo(int codigo)
        {
            return ObterClientePorCodigo(codigo) != null;
        }

        public List<Cliente> ObterTodosClientes()
        {
            return new List<Cliente>(clientes);
        }

        public int TotalClientes()
        {
            return clientes.Count;
        }

        public void LimparClientes()
        {
            clientes.Clear();
        }
    }

    // ========== SERVIÇO DE CHAMADO ==========
    public class ChamadoService
    {
        private List<Chamado> chamados = new List<Chamado>();
        private Queue<Chamado> filaChamados = new Queue<Chamado>();
        private Stack<string> pilhaOperacoes = new Stack<string>();
        private int proximoNumeroChamado = 1;

        public void CadastrarChamado(int codigoCliente, string descricao, PriorityStruct prioridade)
        {
            if (codigoCliente <= 0)
                throw new ArgumentException("Código do cliente deve ser maior que zero");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição não pode estar vazia");

            Chamado chamado = new Chamado(proximoNumeroChamado, codigoCliente, descricao, prioridade);
            chamados.Add(chamado);
            RegistrarOperacao($"Chamado {proximoNumeroChamado} cadastrado");
            proximoNumeroChamado++;
        }

        public void AdicionarChamadoNaFila(int numeroChamado)
        {
            Chamado chamado = ObterChamadoPorNumero(numeroChamado);
            if (chamado == null)
                throw new ArgumentException("Chamado não encontrado");

            filaChamados.Enqueue(chamado);
            RegistrarOperacao($"Chamado {numeroChamado} adicionado à fila");
        }

        public Chamado RetirarPrimeiroChamadoDaFila()
        {
            if (filaChamados.Count == 0)
                throw new InvalidOperationException("Fila vazia");

            Chamado chamado = filaChamados.Dequeue();
            RegistrarOperacao($"Chamado {chamado.Numero} retirado da fila");
            return chamado;
        }

        public Chamado ObterChamadoPorNumero(int numero)
        {
            foreach (Chamado chamado in chamados)
            {
                if (chamado.Numero == numero)
                    return chamado;
            }
            return null;
        }

        public void AlterarStatusChamado(int numeroChamado, StatusChamado novoStatus)
        {
            Chamado chamado = ObterChamadoPorNumero(numeroChamado);
            if (chamado == null)
                throw new ArgumentException("Chamado não encontrado");

            chamado.Status = novoStatus;
            RegistrarOperacao($"Status do chamado {numeroChamado} alterado para {novoStatus}");
        }

        public void RegistrarOperacao(string operacao)
        {
            string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            pilhaOperacoes.Push($"[{timestamp}] {operacao}");
        }

        public string RecuperarUltimaOperacao()
        {
            if (pilhaOperacoes.Count == 0)
                throw new InvalidOperationException("Pilha vazia");

            return pilhaOperacoes.Pop();
        }

        public List<Chamado> ObterTodosChamados()
        {
            return new List<Chamado>(chamados);
        }

        public int TotalChamados()
        {
            return chamados.Count;
        }

        public int TotalNaFila()
        {
            return filaChamados.Count;
        }

        public int TotalOperacoes()
        {
            return pilhaOperacoes.Count;
        }

        public void LimparChamados()
        {
            chamados.Clear();
            filaChamados.Clear();
            pilhaOperacoes.Clear();
            proximoNumeroChamado = 1;
        }
    }

    // ========== SERVIÇO DE PERSISTÊNCIA ==========
    public class PersistenciaService
    {
        private string pastaData = "Dados";
        private string arquivoClientes = "clientes.txt";
        private string arquivoChamados = "chamados.txt";

        public PersistenciaService()
        {
            if (!Directory.Exists(pastaData))
                Directory.CreateDirectory(pastaData);
        }

        public void SalvarChamado(Chamado chamado)
        {
            if (chamado == null)
                throw new ArgumentNullException(nameof(chamado));

            string caminhoArquivo = Path.Combine(pastaData, arquivoChamados);
            string linha = $"{chamado.Numero}|{chamado.CodigoCliente}|{chamado.Descricao}|{chamado.Prioridade.Nivel}|{chamado.Prioridade.Descricao}|{chamado.Status}";

            try
            {
                File.AppendAllText(caminhoArquivo, linha + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw new IOException($"Erro ao salvar chamado: {ex.Message}");
            }
        }

        public void SalvarCliente(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            string caminhoArquivo = Path.Combine(pastaData, arquivoClientes);
            string linha = $"{cliente.Codigo}|{cliente.Nome}|{cliente.Telefone}|{cliente.Email}";

            try
            {
                File.AppendAllText(caminhoArquivo, linha + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw new IOException($"Erro ao salvar cliente: {ex.Message}");
            }
        }

        public List<Cliente> CarregarClientes()
        {
            List<Cliente> clientes = new List<Cliente>();
            string caminhoArquivo = Path.Combine(pastaData, arquivoClientes);

            if (!File.Exists(caminhoArquivo))
                return clientes;

            try
            {
                string[] linhas = File.ReadAllLines(caminhoArquivo);
                foreach (string linha in linhas)
                {
                    if (string.IsNullOrWhiteSpace(linha))
                        continue;

                    string[] partes = linha.Split('|');
                    if (partes.Length >= 4)
                    {
                        Cliente cliente = new Cliente(
                            int.Parse(partes[0]),
                            partes[1],
                            partes[2],
                            partes[3]
                        );
                        clientes.Add(cliente);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Erro ao carregar clientes: {ex.Message}");
            }

            return clientes;
        }

        public List<Chamado> CarregarChamados()
        {
            List<Chamado> chamados = new List<Chamado>();
            string caminhoArquivo = Path.Combine(pastaData, arquivoChamados);

            if (!File.Exists(caminhoArquivo))
                return chamados;

            try
            {
                string[] linhas = File.ReadAllLines(caminhoArquivo);
                foreach (string linha in linhas)
                {
                    if (string.IsNullOrWhiteSpace(linha))
                        continue;

                    string[] partes = linha.Split('|');
                    if (partes.Length >= 6)
                    {
                        PriorityStruct prioridade = new PriorityStruct(
                            int.Parse(partes[3]),
                            partes[4]
                        );

                        Chamado chamado = new Chamado(
                            int.Parse(partes[0]),
                            int.Parse(partes[1]),
                            partes[2],
                            prioridade
                        );

                        if (Enum.TryParse<StatusChamado>(partes[5], out StatusChamado status))
                            chamado.Status = status;

                        chamados.Add(chamado);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Erro ao carregar chamados: {ex.Message}");
            }

            return chamados;
        }

        public void LimparArquivos()
        {
            try
            {
                string caminhoClientes = Path.Combine(pastaData, arquivoClientes);
                string caminhoChamados = Path.Combine(pastaData, arquivoChamados);

                if (File.Exists(caminhoClientes))
                    File.Delete(caminhoClientes);

                if (File.Exists(caminhoChamados))
                    File.Delete(caminhoChamados);
            }
            catch (Exception ex)
            {
                throw new IOException($"Erro ao limpar arquivos: {ex.Message}");
            }
        }
    }
}
