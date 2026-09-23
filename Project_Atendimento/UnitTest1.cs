using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Project_Atendimento
{
    // ========== TESTES [TestMethod] com Padrão Similar ao [Fact] ==========
    [TestClass]
    public class AtendimentoTests
    {
        // T01: Cadastrar um cliente corretamente
        [TestMethod]
        public void T01_DeveCadastrarClienteCorretamente()
        {
            // Arrange
            ClienteService servicoCliente = new ClienteService();

            // Act
            servicoCliente.CadastrarCliente(1, "João Silva", "11987654321", "joao@email.com");

            // Assert
            Assert.AreEqual(1, servicoCliente.TotalClientes());
            Cliente cliente = servicoCliente.ObterClientePorCodigo(1);
            Assert.IsNotNull(cliente);
            Assert.AreEqual("João Silva", cliente.Nome);
            Assert.AreEqual("11987654321", cliente.Telefone);
            Assert.AreEqual("joao@email.com", cliente.Email);
        }

        // T02: Cadastrar um chamado
        [TestMethod]
        public void T02_DeveCadastrarChamado()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            ClienteService servicoCliente = new ClienteService();
            servicoCliente.CadastrarCliente(1, "João Silva", "11987654321", "joao@email.com");

            // Act
            servicoChamado.CadastrarChamado(1, "Sistema não inicia", PriorityStruct.Alta);

            // Assert
            Assert.AreEqual(1, servicoChamado.TotalChamados());
            Chamado chamado = servicoChamado.ObterChamadoPorNumero(1);
            Assert.IsNotNull(chamado);
            Assert.AreEqual("Sistema não inicia", chamado.Descricao);
            Assert.AreEqual(StatusChamado.ABERTO, chamado.Status);
        }

        // T03: Adicionar chamado à fila
        [TestMethod]
        public void T03_DeveAdicionarChamadoNaFila()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            servicoChamado.CadastrarChamado(1, "Problema no login", PriorityStruct.Media);
            int totalAntesFila = servicoChamado.TotalNaFila();

            // Act
            servicoChamado.AdicionarChamadoNaFila(1);

            // Assert
            Assert.AreEqual(totalAntesFila + 1, servicoChamado.TotalNaFila());
        }

        // T04: Retirar o primeiro chamado da fila
        [TestMethod]
        public void T04_DeveRetirarPrimeiroChamadoDaFila()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            servicoChamado.CadastrarChamado(1, "Erro de acesso", PriorityStruct.Critica);
            servicoChamado.AdicionarChamadoNaFila(1);

            // Act
            Chamado chamadoRetirado = servicoChamado.RetirarPrimeiroChamadoDaFila();

            // Assert
            Assert.IsNotNull(chamadoRetirado);
            Assert.AreEqual(1, chamadoRetirado.Numero);
            Assert.AreEqual(0, servicoChamado.TotalNaFila());
        }

        // T05: Adicionar chamado à lista
        [TestMethod]
        public void T05_DeveAdicionarChamadoNaLista()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();

            // Act
            servicoChamado.CadastrarChamado(1, "Dúvida sobre uso", PriorityStruct.Baixa);

            // Assert
            List<Chamado> chamados = servicoChamado.ObterTodosChamados();
            Assert.AreEqual(1, chamados.Count);
            Assert.AreEqual("Dúvida sobre uso", chamados[0].Descricao);
        }

        // T06: Pesquisar chamado existente
        [TestMethod]
        public void T06_DevePesquisarChamadoExistente()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            servicoChamado.CadastrarChamado(1, "Problema no banco de dados", PriorityStruct.Critica);

            // Act
            Chamado chamado = servicoChamado.ObterChamadoPorNumero(1);

            // Assert
            Assert.IsNotNull(chamado);
            Assert.AreEqual(1, chamado.Numero);
            Assert.AreEqual(1, chamado.CodigoCliente);
        }

        // T07: Alterar o status de um chamado
        [TestMethod]
        public void T07_DeveAlterarStatusChamado()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            servicoChamado.CadastrarChamado(1, "Configurar servidor", PriorityStruct.Alta);

            // Act
            servicoChamado.AlterarStatusChamado(1, StatusChamado.EM_ATENDIMENTO);
            Chamado chamado = servicoChamado.ObterChamadoPorNumero(1);

            // Assert
            Assert.AreEqual(StatusChamado.EM_ATENDIMENTO, chamado.Status);
        }

        // T08: Registrar uma operação na pilha
        [TestMethod]
        public void T08_DeveRegistrarOperacaoNaPilha()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            int totalAntesPilha = servicoChamado.TotalOperacoes();

            // Act
            servicoChamado.RegistrarOperacao("Teste de operação");

            // Assert
            Assert.AreEqual(totalAntesPilha + 1, servicoChamado.TotalOperacoes());
        }

        // T09: Recuperar a última operação da pilha
        [TestMethod]
        public void T09_DeveRecuperarUltimaOperacao()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            servicoChamado.RegistrarOperacao("Primeira operação");
            servicoChamado.RegistrarOperacao("Segunda operação");

            // Act
            string operacao = servicoChamado.RecuperarUltimaOperacao();

            // Assert
            Assert.IsTrue(operacao.Contains("Segunda operação"));
        }

        // T10: Salvar um chamado no arquivo
        [TestMethod]
        public void T10_DeveSalvarChamadoNoArquivo()
        {
            // Arrange
            PersistenciaService persistencia = new PersistenciaService();
            Chamado chamado = new Chamado(1, 1, "Teste de persistência", PriorityStruct.Media);

            // Act & Assert
            try
            {
                persistencia.SalvarChamado(chamado);
                List<Chamado> chamadosCarregados = persistencia.CarregarChamados();
                Assert.AreEqual(1, chamadosCarregados.Count);
                persistencia.LimparArquivos();
            }
            catch (Exception)
            {
                persistencia.LimparArquivos();
                throw;
            }
        }
    }

    // ========== TESTES [Theory] COM MÚLTIPLOS CENÁRIOS ==========
    [TestClass]
    public class AtendimentoTheoryTests
    {
        // T11-T15: Cadastrar chamado com dados válidos em diferentes cenários
        [TestMethod]
        [DataRow(1, 1, "Problema de conexão", 1)]
        [DataRow(2, 5, "Erro ao enviar email", 2)]
        [DataRow(3, 10, "Sistema travado", 3)]
        [DataRow(4, 15, "Falha crítica do servidor", 4)]
        [DataRow(5, 20, "Dúvida sobre funcionalidade", 1)]
        public void T11_DeveCadastrarChamadoComDadosValidos(int numero, int codigoCliente, string descricao, int nivelPrioridade)
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();
            PriorityStruct prioridade = new PriorityStruct(nivelPrioridade, TesteHelper.GetDescricaoPrioridade(nivelPrioridade));

            // Act
            servicoChamado.CadastrarChamado(codigoCliente, descricao, prioridade);

            // Assert
            Assert.AreEqual(1, servicoChamado.TotalChamados());
            Chamado chamado = servicoChamado.ObterChamadoPorNumero(1);
            Assert.IsNotNull(chamado);
            Assert.AreEqual(codigoCliente, chamado.CodigoCliente);
            Assert.AreEqual(descricao, chamado.Descricao);
        }
    }

    // ========== TESTES NEGATIVOS ==========
    [TestClass]
    public class AtendimentoNegativeTests
    {
        // Teste negativo: Cliente inexistente
        [TestMethod]
        public void TN01_NaoDevePesquisarClienteInexistente()
        {
            // Arrange
            ClienteService servicoCliente = new ClienteService();

            // Act
            Cliente cliente = servicoCliente.ObterClientePorCodigo(999);

            // Assert
            Assert.IsNull(cliente);
        }

        // Teste negativo: Chamado inexistente
        [TestMethod]
        public void TN02_NaoDevePesquisarChamadoInexistente()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();

            // Act
            Chamado chamado = servicoChamado.ObterChamadoPorNumero(999);

            // Assert
            Assert.IsNull(chamado);
        }

        // Teste negativo: Fila vazia
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TN03_NaoDeveRetirarDaFilaVazia()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();

            // Act
            servicoChamado.RetirarPrimeiroChamadoDaFila();

            // Assert - Exceção esperada
        }

        // Teste negativo: Lista vazia
        [TestMethod]
        public void TN04_ListaChamadosVaziaNoInicio()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();

            // Act
            List<Chamado> chamados = servicoChamado.ObterTodosChamados();

            // Assert
            Assert.AreEqual(0, chamados.Count);
        }

        // Teste negativo: Pilha vazia
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TN05_NaoDeveRecuperarDeStackVazia()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();

            // Act
            servicoChamado.RecuperarUltimaOperacao();

            // Assert - Exceção esperada
        }

        // Teste negativo: Dados inválidos - Cliente com código zero
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TN06_NaoDeveCadastrarClienteComCodigoZero()
        {
            // Arrange
            ClienteService servicoCliente = new ClienteService();

            // Act
            servicoCliente.CadastrarCliente(0, "Nome", "123", "email@test.com");

            // Assert - Exceção esperada
        }

        // Teste negativo: Dados inválidos - Cliente com nome vazio
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TN07_NaoDeveCadastrarClienteComNomeVazio()
        {
            // Arrange
            ClienteService servicoCliente = new ClienteService();

            // Act
            servicoCliente.CadastrarCliente(1, "", "123", "email@test.com");

            // Assert - Exceção esperada
        }

        // Teste negativo: Dados inválidos - Cliente duplicado
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TN08_NaoDeveCadastrarClienteComCodigoDuplicado()
        {
            // Arrange
            ClienteService servicoCliente = new ClienteService();
            servicoCliente.CadastrarCliente(1, "João", "123", "joao@test.com");

            // Act
            servicoCliente.CadastrarCliente(1, "Maria", "456", "maria@test.com");

            // Assert - Exceção esperada
        }

        // Teste negativo: Arquivo inexistente
        [TestMethod]
        public void TN09_DeveCarregarListaVaziaSeArquivoNaoExiste()
        {
            // Arrange
            PersistenciaService persistencia = new PersistenciaService();
            persistencia.LimparArquivos();

            // Act
            List<Cliente> clientes = persistencia.CarregarClientes();

            // Assert
            Assert.AreEqual(0, clientes.Count);
        }

        // Teste negativo: Alterar chamado inexistente
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TN10_NaoDeveAlterarStatusDeChamadoInexistente()
        {
            // Arrange
            ChamadoService servicoChamado = new ChamadoService();

            // Act
            servicoChamado.AlterarStatusChamado(999, StatusChamado.RESOLVIDO);

            // Assert - Exceção esperada
        }
    }

    // ========== CLASSE AUXILIAR PARA TESTES ==========
    public static class TesteHelper
    {
        public static string GetDescricaoPrioridade(int nivel)
        {
            switch (nivel)
            {
                case 1: return "Baixa";
                case 2: return "Média";
                case 3: return "Alta";
                case 4: return "Crítica";
                default: return "Desconhecida";
            }
        }
    }
}
