using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;


namespace cdoc
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(70, 40);
            Principal p = new Principal();
            //p.ini();
            p.menu();
        }
    }

    class Principal : Object
    {
        Regex padrao = new Regex(@"(/\*\*\*(.|\n)*?\*\*\*/)|(<!--(.|\n)*?-->)", RegexOptions.None);
        
        Regex padraoTroca = new Regex(@"/\*\*\*|\*\*\*/|\<!--|-->", RegexOptions.None);

        string blocoInicio;

        string cabecalhoLimpo;

        string diretorio=@"d:\testecdoc";


        #region variaveis html
        // ################################################
        // VARIÁVEIS DE MONTAGEM HTML #####################
        // ################################################

        string html_inicio = "";
        #region html_fim

        string html_fim = @"
                       </div>
                </div>
            </div> ";

        #endregion

        #region filetree inicio

        string fileTree_inicio = @"<!-- Tab panes -->
                <div class='tab-content'>
                    <div role = 'tabpanel' class='tab-pane fade in active mytab' id='filetree'>
                        <span class='glyphicon glyphicon-folder-open' aria-hidden='true'></span>
                        <p />";

        #endregion

        string projeto_inicio = @"
            <div class='collapse' id='collapseExample'>
                <div class='well'>
                    
";
        string projeto_meio = "";
        string projeto_fim = @"
           
                </div>
            </div>
            <hr />
            <!-- tab init -->
            <div>
                <!-- Nav tabs -->
                <ul class='nav nav-tabs' role='tablist'>
                    <li role = 'presentation' class='active'><a href = '#filetree' aria-controls='filetree' role='tab' data-toggle='tab'>FileTree</a></li>
                    <li role = 'presentation' ><a href='#funcoes' aria-controls='funcoes' role='tab' data-toggle='tab'>Lista de Classes, Funções e Blocos</a></li>
                    <li role = 'presentation' ><a href='#todo' aria-controls='todo' role='tab' data-toggle='tab'>Todo List</a></li>
                </ul>
";

        string fileTree_meio = "";
        string fileTree_fim = "";

        string list_inicio = "<div role='tabpanel' class='tab-pane fade mytab' id='funcoes'>";
        string list_meio = "";
        string list_fim = "</div>";

        string todo_inicio = @"
        <div role = 'tabpanel' class='tab-pane fade mytab' id='todo'>
            <ul class='list-group'>
            ";
        string todo_meio = "";
        string todo_fim = "</ul></div>";



        // profundidade da filetree, para saber quantos span space deve ser aplicado nos subfolders
        int profundidade = 0;

        // salva a ultima profundidade para poder saber quando saiu de um diretorio e fechar a div
        int profundidadeOld = 0;

        // variavel que cria as divs ft'n' das pastas
        int contadorDePastas = 0;

        // variável que nomeia os modals de imagens e arquivos (popups da filetree)
        int contadorDeModals = 0;

        //Armazena temporariamente o arquivo limpo sem comentários cdoc para ser adicionado ao html
        string ARQUIVO_PURO = "";

        // verifica se o arquivo é válido para colocar os icones ao lado
        bool isValid;

        // verifica se o projeto possui um nome
        bool hasName = false;

        // arraylist de arquivos ignorados
        String[] ignore = new String[1000];




        // #################################################
        // FIM MONTAGEM HTML ###############################
        // #################################################
        #endregion


        public void menu()
        {
            // cria o menu inicial
            this.titulo();

            if (diretorio != "")
            {
                Console.WriteLine("----------\nDiretório atual:\n" + diretorio+ "\n----------\n\n");
            }
            
            Console.WriteLine("::MENU::\n");
            Console.WriteLine("[1] - Escolher diretório");
            Console.WriteLine("[2] - Criar Documentação");
            Console.WriteLine("[3] - Sair");
            Console.WriteLine("[4] - Versão\n");
            Console.Write("Digite sua escolha: ");
            string menu=Console.ReadLine();

            switch (menu)
            {
                case "1":
                    this.escolherDiretorio();
                    break;

                case "2":
                    this.criar();
                    break;
            }
        }

        public void titulo()
        {
            Console.Clear();
            Console.WriteLine("\n\nCedros Documenter\t\t\t\t [cedrosdev.com.br]");
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("\t\t\t\t\t\t\t       v1.0\n");
        }

        public void escolherDiretorio()
        {
            this.titulo();
            Console.WriteLine("Escolher Diretório\n");
            Console.Write("Digite ou cole o caminho da pasta do seu projeto:\n");
            diretorio=Console.ReadLine();
            this.menu();
            
        }

        public void criar()
        {
            this.titulo();
            //entra no diretorio , usar try
            Console.WriteLine("CDOC está trabalhando, aguarde.\n\n");
            try
            {
                DirectoryInfo dir = new DirectoryInfo(diretorio);

                // #######################
                // cdocignore
                // #######################
                #region cdocignore
                FileInfo ig = new FileInfo(diretorio + @"\.cdocignore");
                if (File.Exists(ig.FullName))
                {
                    FileStream streamIg = new FileStream(ig.FullName, FileMode.Open, FileAccess.Read);
                    StreamReader readerIg = new StreamReader(streamIg);
                    string textoIg = readerIg.ReadToEnd();
                    readerIg.Close();

                    string p = @"\n|\r";

                    ignore = Regex.Split(textoIg, p);

                }
                else
                {
                    // cria
                    FileStream streamIg = new FileStream(ig.FullName, FileMode.Create, FileAccess.Write);
                    StreamWriter writerIg = new StreamWriter(streamIg);
                    writerIg.Write(@"_todo.txt
_project.txt
_footer.txt
.cdocignore");
                    writerIg.Close();

                    ignore[0] = "_todo.txt";
                    ignore[1] = "_project.txt";
                    ignore[2] = ".cdocignore";
                    ignore[3] = "_footer.txt";          
                }

                    #endregion

                    //varre o diretorio principal
                    foreach (FileInfo a in dir.GetFiles())
                {
                    //Console.WriteLine(arq2.Name);
                    this.fileTree(a, null);
                }
                //varre os subdiretorios
                recursivo(dir);

                //imprime a saida :: teste
                //Console.WriteLine(fileTree_meio);

                //testa a profundidade para saber quantas divs devem ser fechadas
                for(int i = 0; i <= profundidade; i++)
                {
                    fileTree_fim += "</div>";
                }
                //Console.WriteLine(fileTree_fim);

                // #######################
                // todo
                // #######################
                #region todo
                FileInfo todo = new FileInfo(diretorio + @"\_todo.txt");
                if (File.Exists(todo.FullName))
                {



                    FileStream streamTodo = new FileStream(todo.FullName, FileMode.Open, FileAccess.Read);
                    StreamReader readerTodo = new StreamReader(streamTodo);
                    string textoTodo = readerTodo.ReadToEnd();
                    readerTodo.Close();
                    // textoTodo contem o conteudo da todo
                    // Console.WriteLine(textoTodo);

                    string p = @"\n";
                    string p2 = @"--";
                    Regex ini = new Regex("^#", RegexOptions.None);
                    String[] elements = Regex.Split(textoTodo, p);

                    for(int i = 0; i < elements.Length; i++)
                    {
                        if (ini.IsMatch(elements[i]))
                        {
                            String[] elements2 = Regex.Split(elements[i], p2);
                            string percent = ini.Replace(elements2[0],"");
                            // Console.WriteLine(percent + " ----- " + elements2[1]);

                            //descobre se o proximo elemento começa com # ou se é uma descrição do atual
                            string desc = "";
                            try {

                                if (!ini.IsMatch(elements[i + 1]))
                                {

                                    desc += elements[i + 1];
                                    int a = 2;
                                    while (!ini.IsMatch(elements[i + a]))
                                    {
                                        desc += "<br />"+elements[i + a];
                                        a++;
                                    }
                                }                                
                            }
                            catch (Exception x)
                            {
                                //
                            }
                            int pp = int.Parse(percent);
                            string color="";
                            if (pp >= 0 && pp < 40) color = "danger";
                            else if (pp >= 40 && pp < 80) color = "warning";
                            else if (pp >= 80 && pp < 100) color = "info";
                            else if (pp >= 100) color = "success";

                            //Console.WriteLine(color);

                            todo_meio += @"

                            <li class='list-group-item'>
                                
                                <h4>" + elements2[1] + @"</h4><br />

                                <div class='progress'>
                                  <div class='progress-bar progress-bar-" + color+@"' role='progressbar' aria-valuenow='"+pp+@"' aria-valuemin='0' aria-valuemax='100' style='width: "+pp+ @"%;min-width: 2em;'>
                                    " + pp+@"%
                                  </div>
                                </div>
                                <p />
                                
                                "+desc+@"
                            </li>
    
                                ";



                        }
                    }


                }
                #endregion


                // #######################
                // detalhes
                // #######################
                #region detalhes

                string projeto_nome = "NoName Project";

                FileInfo projeto = new FileInfo(diretorio + @"\_project.txt");
                if (File.Exists(projeto.FullName))
                {
                    FileStream streamProjeto = new FileStream(projeto.FullName, FileMode.Open, FileAccess.Read);
                    StreamReader readerProjeto = new StreamReader(streamProjeto);
                    string textoProjeto = readerProjeto.ReadToEnd();
                    readerProjeto.Close();
                    // textoProjeto tem o conteudo do projeto.txt

                    //Console.WriteLine(this.codeList(textoProjeto,"projeto"));

                    projeto_meio = this.codeList(textoProjeto, "projeto");
                    projeto_nome = this.codeList(textoProjeto, "projeto_nome");

                    if (!hasName) projeto_nome = "NoName Project";
                }


                    #region html_inicio

                    html_inicio = @"<!DOCTYPE html>
<html lang='en'>
    <head>
        <meta charset='utf-8'>
        <meta http-equiv='X-UA-Compatible' content='IE=edge'>
        <meta name='viewport' content='width=device-width, initial-scale=1'>
        <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
        <title>CDOC : "+projeto_nome+ @"</title>
        <link rel='icon' type='image/png' href='images/cdoc-icon.png'/>
              <script src='js/jquery.js'></script>
<script src='js/motor.js'></script>
        
        <link href='css/bootstrap.min.css' rel='stylesheet'>
        <script src='js/bootstrap.js'></script>
        <link rel='stylesheet' href='highlight/styles/color-brewer.css'>
        <script src='highlight/highlight.pack.js'></script>
        <script>
        hljs.initHighlightingOnLoad();
 $(function() {
     $('[data-toggle=" + '\u0022' + "tooltip" + '\u0022' + @"]').tooltip();
     $('#myTabs a').click(function(e)
        {
            e.preventDefault();
         $(this).tab('show');
        });

 })
 var isOpen = [];

    function abrir(x)
    {
        if (isOpen[x])
        {
         $('#' + x).hide();
            isOpen[x] = false;
         $('#folder' + x).removeClass('glyphicon glyphicon-folder-open').addClass('glyphicon glyphicon-folder-close');
        }
        else
        {
         $('#' + x).show();
            isOpen[x] = true;
         $('#folder' + x).removeClass('glyphicon glyphicon-folder-close').addClass('glyphicon glyphicon-folder-open');
        }
    }

    function hyper(x)
    {

        window.location = x;
    }
        </script>
        
        <style type = 'text/css' >
        .mytab {
        padding:10px;
        }
        .space{
        padding-left: 20px;
        }
        .divTree{
        padding: 0px;
       
        display: none;
        
        }
        pre{
    background-color: transparent;
    padding: 0;
    margin: 0;
    border:0;
    }
        </style>
        <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
        <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
        <!--[if lt IE 9]>
        <script src = 'https://oss.maxcdn.com/html5shiv/3.7.2/html5shiv.min.js' ></script>
        <script src= 'https://oss.maxcdn.com/respond/1.4.2/respond.min.js' ></script>
        <![endif]-->
    </head>
    <body>
        <div id= 'conteudoHyper'></div>
        <div class='container'>
            <div class='page-header'>
                <h1>#CDOC <small>Cedros Documenter </small></h1>
            </div>
            <h3>"+projeto_nome+@"
            <button class='btn btn-primary' type='button' data-toggle='collapse' data-target='#collapseExample' aria-expanded='false' aria-controls='collapseExample'>
            Exibir Detalhes do Projeto
            </button> </h3>
            ";

                    #endregion


                

                #endregion

                // #######################
                // rodape
                // #######################
                #region rodape
                FileInfo footer = new FileInfo(diretorio + @"\_footer.txt");
                string footerString = "";
                if (File.Exists(footer.FullName))
                {
                    FileStream streamFooter = new FileStream(footer.FullName, FileMode.Open, FileAccess.Read);
                    StreamReader readerFooter = new StreamReader(streamFooter);
                    footerString = readerFooter.ReadToEnd();
                    readerFooter.Close();
                }

                string data=System.DateTime.Now.Day + "/" + System.DateTime.Now.Month + "/" + System.DateTime.Now.Year + " às " + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute;

                string html_footer = @"
                <div class='container'>
                        <div class='well'>
                            <h6>
                                "+footerString+@"<br />
                                <small>
                                    (c) Cedros Development 2016-2020 | Feito com Cedros Documenter rodado em "+data+@" | <a href = 'http://www.cedrosdev.com.br/cdoc' target='blank_'>cedrosdev.com.br/cdoc</a>
                                </small>
                            </h6>
                        </div>
                    </div>
";


                #endregion



                // #####################################
                // modal das imagens
                // #####################################
                html_fim += @"<!--Modal Imagem-->
                    <div class='modal fade' id='modalImagens' tabindex='-1' role='dialog' aria-labelledby='myModalLabel'>
                      <div class='modal-dialog' role='document'>
                        <div class='modal-content'>
                          <div class='modal-header'>
                            <button type = 'button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>
                            <h4 class='modal-title' id='modalImagemTitulo'></h4>
                          </div>
                          <div class='modal-body'>
                            <img src = '' width='100%' id='imgModal'>
                          </div>
                          <div class='modal-footer'>
                            <button type = 'button' class='btn btn-danger' data-dismiss='modal'>Close</button>
                            <button type = 'button' class='btn btn-info' disabled='disabled'>Visualizar HyperCode</button>
                          </div>
                        </div>
                      </div>
                    </div>
                    ";


                // MONTA O ARQUIVO E SALVA
                string resultadoFinal = html_inicio + projeto_inicio + projeto_meio + projeto_fim + fileTree_inicio + fileTree_meio + fileTree_fim + list_inicio + list_meio + list_fim + todo_inicio + todo_meio + todo_fim + html_fim + html_footer + "</body></html>";

                FileInfo arq = new FileInfo(@"D:\PROGRAMACAO\VISUAL STUDIO\cdoc\htmlCdoc\index.html");
                if (File.Exists(arq.FullName))
                {
                    arq.Delete();
                    //Console.WriteLine("deletado.");
                }

                FileStream fs = new FileStream(@"D:\PROGRAMACAO\VISUAL STUDIO\cdoc\htmlCdoc\index.html", FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter sw = new StreamWriter(fs);
                sw.Write(resultadoFinal);
                sw.Close();

                Console.WriteLine("Documentação gerada com sucesso.\nPressione [enter] para voltar ao menu inicial.");



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
            this.menu();
        }

        public void recursivo(DirectoryInfo dir)
        {
            DirectoryInfo[] dirs = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            
            if (dirs.Length != 0)
            {
                for(int i = 0; i < dirs.Length; i++)
                {
                    //Console.WriteLine("--->"+dirs[i].Name);
                    this.fileTree(null, dirs[i]);
                    foreach (FileInfo arq in dirs[i].EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                    {
                        //Console.WriteLine(arq);
                        this.fileTree(arq, null);
                    }
                        recursivo(dirs[i]);
                }
            }
            
        }

        public void fileTree(FileInfo arq,DirectoryInfo dir)
        {

            string spacos = "";
            
            if (dir == null)
            {
                bool pular = false;

                //cdocignore
                for(int i = 0; i < ignore.Length; i++)
                {
                    if (ignore[i] == arq.Name) pular = true;
                }
                
                if (!pular)
                {
                    //lista de imagens aceitas
                    #region imagens
                    if (arq.Extension == ".jpg" || arq.Extension == ".gif" || arq.Extension == ".png")
                    {

                        //implementa o contador de modals
                        contadorDeModals++;

                        //Console.WriteLine(arq.Name+" [imagem]" + " - " + profundidade);

                        for (int i = 0; i < profundidade; i++)
                        {
                            spacos += "<span class='space'></span>";
                        }
                        fileTree_meio += spacos + "<img src = 'images/filetree.gif'> " + arq.Name + " <a href = \"javascript:imagem('"+arq.Name+"','"+arq.FullName+"');\" data-toggle = 'modal' data-target = '#modalImagens'><span class='glyphicon glyphicon-eye-open' aria-hidden='true' alt='Visualização Simples' title='Visualização Simples'></span></a><br>";

                       

                        

                    }
                    #endregion

                    //lista de arquivos aceitos
                    #region arquivos
                    else if (arq.Extension == ".js" || arq.Extension == ".php" || arq.Extension == ".html")
                    {
                        // if (arq.Extension == ".html") 
                        // envia o arquivo para ser tratado e analisado, criando o hypercode e alimentando as variaveis
                        // para serem usadas na contrução da index.htm na lista de classes, funções e blocos

                        isValid = this.hypercode(arq);
                        // a partir daqui, ARQUIVO_PURO contem temporariamente o arquivo sem comentario nenhum
                        // adicionar ao html
                        // pensar em html entites para não ter problemas futuros
                        // posso passar o ARQUIVO_PURO em outra função
                        // exemplo ARQUIVO_PURO=transformaHTML(ARQUIVO_PURO);


                        //implementa o contador de modals
                        contadorDeModals++;

                        //Console.WriteLine(arq.Name + " [código]" + " - " + profundidade);
                        for (int i = 0; i < profundidade; i++)
                        {
                            spacos += "<span class='space'></span>";
                        }

                        // se o arquivo começar com #cdoc coloca os icones ao lado e cria o modal, se não, não.
                        if (isValid)
                        {
                            fileTree_meio += spacos + @"

            <div>
                <img src = 'images/filetree.gif'> " + arq.Name + @" 
                <a href = 'javascript:void(0);' data-toggle = 'modal' data-target = '#" + contadorDeModals + @"'>
                    <span class='glyphicon glyphicon-eye-open' aria-hidden='true' alt='Visualização Simples' title='Visualização Simples'></span>
                </a> 
                <a href = " + '\u0022' + "javascript:hyper('" + arq.Name + @".html');void(0);" + '\u0022' + @">
                    <span class='glyphicon glyphicon-list-alt' aria-hidden='true' alt='Hyper Code' title='Hyper Code' data-toggle='modal' data-target='#hypercode'></span>
                </a>
            </div>

";
                            // toda vez que identificamos um arquivo devemos criar um modal
                            // não quer dizer que terá um hypercode, pois para isso precisa de um inicio #cdoc válido
                            // mas mesmo não sendo válido seu código fonte deve ser mostrado

                            //todo :: criar teste para saber se tem hypercode

                            html_fim += @"<!--Modal arquivo-->
                    <div class='modal fade' id='" + contadorDeModals + @"' tabindex='-1' role='dialog' aria-labelledby='myModalLabel'>
                      <div class='modal-dialog' role='document' style='width:1000px'>
                        <div class='modal-content' style='width:1000px'>
                          <div class='modal-header' style='width:1000px'>
                            <button type = 'button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>
                            <h4 class='modal-title' id='myModalLabel'>" + arq.Name + @"</h4>
                          </div>
                          <div class='modal-body' style='width:1000px'>
                            <pre><code>" +
                                    ARQUIVO_PURO
                                    + @"
                            </code></pre>
                          </div>
                          <div class='modal-footer' style='width:1000px'>
                            <button type = 'button' class='btn btn-danger' data-dismiss='modal'>Close</button>
                            <button type = 'button' class='btn btn-info' onClick=" + '\u0022' + "javascript:hyper('" + arq.Name + @".html');void(0);" + '\u0022' + @">Visualizar HyperCode</button>
                          </div>
                        </div>
                      </div>
                    </div>";
                        }
                        else
                        {
                            // arquivo conhecido porem sem documentação cdoc válida
                            fileTree_meio += "<div>" + spacos + "<img src = 'images/filetree.gif'> " + arq.Name + "</div>";
                        }

                    }
                    #endregion

                    //lista de arquivos desconhecidos pelo cdoc
                    #region arquivos desconhecidos
                    else
                    {
                        // Console.WriteLine(arq.Name + " [desconhecido]" + " - " + profundidade);
                        for (int i = 0; i < profundidade; i++)
                        {
                            spacos += "<span class='space'></span>";
                        }
                        fileTree_meio += "<div>" + spacos + "<img src = 'images/filetree.gif'> " + arq.Name + "</div>";
                        //Console.WriteLine(spacos+"< div >< img src = 'images / filetree.gif' > " + arq.Name + "</ div >");

                    }
                    #endregion

                }
            }
            else
            {
                // se for folder
                int p = Path.GetFullPath(dir.FullName).Split(Path.DirectorySeparatorChar).Length;
                int d = Path.GetFullPath(diretorio).Split(Path.DirectorySeparatorChar).Length;
                profundidade = p - d;
                if (profundidade <= profundidadeOld)
                {
                    for (int i = profundidade; i <= profundidadeOld; i++)
                    {
                        // se a profundidade atual for menor que a profundidade antiga quer dizer que saimos de uma pasta, logo temos
                        // que fechar a div
                        fileTree_meio += @"

<!-- ############################# FECHEI UMA PASTA ############################# -->

</div>

<!-- ############ -->

";

                    }
                }
                profundidadeOld = profundidade;
                contadorDePastas++;

                // Console.WriteLine("->" + dir.Name+" - "+profundidade);
                for (int i = 1; i < profundidade; i++)
                {
                    spacos += "<span class='space'></span>";
                }
                fileTree_meio += spacos+ @"<img src='images/filetree.gif'>
                                   <span class='glyphicon glyphicon-folder-close' aria-hidden='true' id='folderft"+contadorDePastas+@"'></span>
                                <a href = " + '\u0022' + "javascript:abrir('ft" + contadorDePastas+ "');void(0);" + '\u0022' + " class='link'>" + dir.Name+"</a><br />";

                //abre a div
                fileTree_meio += @"

<!-- ############### ABRI UMA PASTA ############################## -->

<div id='ft"+contadorDePastas+@"' class='divTree'>

<!-- ########### -->

";



            }
        }

        public bool hypercode(FileInfo arquivoOriginal)
        {
            


            //carrega o arquivo
            FileStream fs = new FileStream(arquivoOriginal.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            string conteudo = sr.ReadToEnd();
            sr.Close();

            
            //-- fim função arquivo
            foreach (Match cdoc in padrao.Matches(conteudo))
            {
              
                // o primeiro ( e unico) cdoc.Valeu é o cabeçalho
                // o comando a baixo retira os caracteres cdoc, deixando apenas as hashtags.
                // resultado = cabeçalho limpo
                string resultado = padraoTroca.Replace(cdoc.Value, "");
               

                Regex inicio = new Regex("^(\n|\\s)*#cdoc", RegexOptions.IgnoreCase);
                if (inicio.IsMatch(resultado))
                {
                    

                    //Console.WriteLine(arquivoOriginal.FullName+" --- > Inicio válido\n\n");
                    blocoInicio = resultado;
                    /** registro o bloco inicial dentro da variável resultado **/

                    //tenho que registrar todo o resto do arquivo em outra variável para ser analizada
                    Regex tiraInicio = new Regex(@"(^/\*\*\*(.|\n)*?\*\*\*/)|(^<!--(.|\n)*?-->)", RegexOptions.None);
                    // o '^' significa a correspondencia no inicio da expressão.
                    string arquivo = tiraInicio.Replace(conteudo, "");


                    // chama a função lista para criar a lista de funções e blocos a partir da variável arquivo
                    this.lista(arquivo,arquivoOriginal);


                    //essa variavel armazena temporariamente o arquivo sem nenhum comentario para ser usado no html
                    //começar a pensar em html entites, php, html usam tags que podem comprometer o parser
                    ARQUIVO_PURO = padrao.Replace(conteudo, "");
                    ARQUIVO_PURO = this.formata(ARQUIVO_PURO);

                    // #########################################
                    // CRIAÇÃO DO HYPERCODE
                    // #########################################

                    // var conteudo retem o arquivo original

                    //vamos fazer um split

                    String p = @"/\*\*\*|\*\*\*/|<!--|-->";

                    String[] elements = Regex.Split(conteudo, p);

                    string saida = "";

                    string tmp = "";

                    Regex initFunction = new Regex(@"\[init function\]", RegexOptions.None);
                    Regex endFunction = new Regex(@"\[end function\]", RegexOptions.None);
                    Regex initBlock = new Regex(@"\[init block\]", RegexOptions.None);
                    Regex endBlock = new Regex(@"\[end block\]", RegexOptions.None);
                    Regex initBotao = new Regex(@"#>", RegexOptions.None);
                    Regex endBotao = new Regex(@"<#", RegexOptions.None);
                    string a;
                    
                    for (int i = 0; i < elements.Length; i++)
                    {
                       
                        if (i == 1)
                        {
                            saida += @"<div class='panel panel-warning'><div class='panel-heading'>" + codeList(elements[1], "titulo") +
                                @"</div><div class='panel-body'><pre><code>";
                        }
                        else if (initFunction.IsMatch(elements[i]))
                        {

                            saida += @"</code></pre><div class='panel panel-primary noMargin'><div class='panel-heading'>" + codeList(elements[i], "funcao") +
                                        @"</div><div class='panel-body noMargin'><pre><code>";

                        }
                        else if (endFunction.IsMatch(elements[i]))
                        {
                            saida += @"</code></pre></div></div><pre><code>";
                        }
                        else if (initBlock.IsMatch(elements[i]))
                        {
                            saida += @"</code></pre><div class='panel panel-primary'><div class='panel-body'><pre><code>";
                            tmp = elements[i];
                        }
                        else if (endBlock.IsMatch(elements[i]))
                        {


                            saida += @"</code></pre></div><div class='panel-footer'>" + codeList(tmp, "bloco") +
                                @"</div></div><pre><code>";
                        }
                        else if (initBotao.IsMatch(elements[i]))
                        {

                            a = elements[i].Replace("#>", "");
                            saida += @"</code></pre><button type='button' class='btn btn-default myButton' data-toggle='tooltip' data-placement='right' title='" + a + @"'>";
                        }
                        else if (endBotao.IsMatch(elements[i]))
                        {

                            a = elements[i].Replace("<#", "");
                            saida += @"<span class='glyphicon glyphicon-search' aria-hidden='true'></span></button><pre><code>";
                        }
                        else if (i == (elements.Length - 1))
                        {
                            saida += this.formata(elements[i].Trim());
                            saida += @"</code></pre></div></div>";
                        }

                        else
                        {
                            saida += this.formata(elements[i].Trim());
                            
                        }
                    }
                    string inicioHTML = @"<!DOCTYPE html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
    <title>CDOC : "+arquivoOriginal.Name+ @"</title>
<link rel='icon' type='image/png' href='images/cdoc-icon.png'/>
    <script src='js/jquery.js'></script>
    
    <link href='css/bootstrap.min.css' rel='stylesheet'>
    <script src='js/bootstrap.js'></script>
    <link rel='stylesheet' href='highlight/styles/color-brewer.css'>
    <script src='highlight/highlight.pack.js'></script>
    <script>
    hljs.initHighlightingOnLoad();
    $(function () {
    $('[data-toggle=" + '\u0022' + "tooltip" + '\u0022' + @"]').tooltip()
    })
    </script>
    
    <style type='text/css'>
    .myButton {
        margin-left:20px;
    }
    pre{
    background-color: transparent;
    padding: 0;
    margin: 0;
    border:0;
    }
    .noMargin {margin:0;padding:0;}
    .btn, .btn-default{
    font-family: Courier New;
    font-size: 12px;
    font-weight: bold;
    color:#0000cc;
    }
    .tooltip-inner{
    background-color: #ddd;
    color: #444;
    }
    .noCode{
    color:red;
    }
    .table2 {
    font-size:10px;
    color:#555;
    margin:0;
    }
    .table3 {
    font-size:10px;
    color:#eee;
 margin:0;

    }
    </style>
    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
    <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
    <!--[if lt IE 9]>
    <script src='https://oss.maxcdn.com/html5shiv/3.7.2/html5shiv.min.js'></script>
    <script src='https://oss.maxcdn.com/respond/1.4.2/respond.min.js'></script>
    <![endif]-->
  </head>
  <body>
    
    <div class='container'>
      <div class='page-header'>
        <h1>#CDOC <small>Cedros Documenter </small></h1>
      </div>
      <div class='alert alert-success' role='alert'>"+arquivoOriginal.Name+ @"
 <a href=" + '\u0022' + @"javascript: window.location = 'index.html'" + '\u0022' + @">

           <span title = 'Home' alt = 'Home' class='glyphicon glyphicon-home' style='font-size:20px;margin:10px;' aria-hidden='true'>
</span></a>

        <a href = 'javascript:void(0);' data-toggle='modal' data-target='#myModal'>

        <span alt = 'Visualizar código puro' title='Visualizar código puro' class='glyphicon glyphicon-eye-open' style='font-size:20px;margin:10px;' aria-hidden='true'></span></a></div>
";
                    string meioHTML = saida;
                    string fimHTML = @" 
<!-- Modal-->
<div class='modal fade' id='myModal' tabindex='-1' role='dialog' aria-labelledby='myModalLabel'>
    <div class='modal-dialog' role='document' style='width:1000px'>
    <div class='modal-content' style='width:1000px'>
      <div class='modal-header' style='width:1000px'>
        <button type = 'button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>
        <h4 class='modal-title' id='myModalLabel'>"+arquivoOriginal.Name+@"</h4>
      </div>
      <div class='modal-body'>
        <pre><code>

" + ARQUIVO_PURO+@"
        </code></pre>
      </div>
      <div class='modal-footer'>
      </div>
    </div>
  </div>
</div>

</body></html>";

                    FileInfo arq = new FileInfo(@"D:\PROGRAMACAO\VISUAL STUDIO\cdoc\htmlCdoc\"+arquivoOriginal.Name+".html");
                    if (File.Exists(arq.FullName))
                    {
                        arq.Delete();
                    }

                    fs = new FileStream(@"D:\PROGRAMACAO\VISUAL STUDIO\cdoc\htmlCdoc\"+arquivoOriginal.Name+".html", FileMode.OpenOrCreate, FileAccess.Write);

                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(inicioHTML + meioHTML + fimHTML);
                    sw.Close();



                    // #########################################
                    // FIM HYPERCODE
                    // #########################################









                    return true;

                }
               
            }
            return false;
        }

        //função que vai criar a lista de funções e blocos a partir da função hypercode
        public void lista(string arquivo, FileInfo arquivoOriginal)
        {
            // Console.WriteLine(arquivoOriginal.Name +" ###############\n"+arquivo+"#################\n\n\n\n\n");


            //#########################################################################################

            //RETRATAMENTO DA VARIÁVEL ARQUIVO

            //[ INIT ]

            //pegas as funções 
            Regex padraoFuncoes = new Regex(@"/\*\*\*(\n|\s)*\[init function\](.|\n)*?\[end function\]\s*\*\*\*/", RegexOptions.None);
            foreach (Match funcoes in padraoFuncoes.Matches(arquivo))
            {
                //funcoes guarda todas as funções do arquivo
                //guardar em array ?
                //não há problema em pegar a função dentro do bloco desde que sejam aplicadas as tags corretas

                //retira os comandos cdoc da variavel funcoes
                //var funcaoLimpa retem a função sem comentarios
                Regex retira = new Regex(@"/\*\*\*(\n|\s)*\[init function\](.|\n)*?\*\*\*/|/\*\*\*(\n|\s)*\[end function\](.|\n)*?\*\*\*/", RegexOptions.None);
                string funcaoLimpa = retira.Replace(funcoes.Value, "");


                //pega o cabeçalho da função
                Regex cabecalho = new Regex(@"/\*\*\*(\n|\s)*\[init function\](.|\n)*?\*\*\*/", RegexOptions.None);

                foreach (Match funcaoCabecalho in cabecalho.Matches(funcoes.Value))
                {
                    //funcaoCabecalho.Value detem o cabeçalho agora
                    //temos que limpar o cabeçalho
                    Regex limpaCabecalho = new Regex(@"/\*\*\*(\n|\s)*\[init function\](\n|\s)*|(\n|\s)*\*\*\*/", RegexOptions.None);
                    cabecalhoLimpo = limpaCabecalho.Replace(funcaoCabecalho.Value, "");
                    //cabecalhoLimpo detem o cabeçalho sem os comandos cdoc

                }
                /** ###
                 * agora eu tenho
                 * funcaoLimpa que retem a funcao em si sem os comando cdoc
                 * cabecalhoLimpo que retem o cabecalho sem os comando cdoc
                 * todo: gerar codigo html
                 * tem que fazer a substituição no arquivo original ou criar uma variavel nova chamada saida
                 * agora tenho cabecalhoLimpo com o cabeçalho da função - que pode ser tratado com split - e tenho 
                 * funcao limpa com o corpo da funcao.
                 * o que deve ser feito depois é recriar o arquivo apenas com substituições e usar essas variaveis para criar o index
                 * das funções
                 * 
                 */

                // Console.WriteLine(arquivoOriginal.Name + " --> \n\n" + cabecalhoLimpo + "\n\n" + funcaoLimpa + "\n\n\n");

                // ---------------------------------------------------------------------------------------------------
                // tem que tratar o cabeçalho para se extrair o nome e a descrição da função
                // ---------------------------------------------------------------------------------------------------

                String r = @":|\n";
                // divide o arquivo se encontrar ':' ou '\n'
                // se o code[i] for igual a 'nome', code[i+1] será 'kaue'

                String[] codes = Regex.Split(cabecalhoLimpo, r);
                string nome = "";
                string desc = "";

                for (int k = 0; k < codes.Length; k++)
                {

                    switch (codes[k].Trim().ToLower())
                    {


                        case "#name":
                            nome = codes[k + 1];
                            break;

                        case "#desc":
                            desc = codes[k + 1];
                            break;
                    }
                }

                // ---------------------------------------------------------------------------------------------------
                // agora temos nome e desc
                // ---------------------------------------------------------------------------------------------------
                contadorDeModals++;
                list_meio += @"

                <div class='list-group' data-toggle='modal' data-target='#"+contadorDeModals+@"'>
                    <a href = '#' class='list-group-item '>
                        <h4 class='list-group-item-heading'>
                            "+arquivoOriginal.Name+" > "+nome+@"
                        </h4>
                        <p class='list-group-item-text'>
                            "+desc+@"
                        </p>
                    </a>
                </div>

                ";

                // cria o modal
                html_fim += @"

<!-- Modal Lista-->
<div class='modal fade' id='"+contadorDeModals+ @"' tabindex='-1' role='dialog' aria-labelledby='myModalLabel'>
  <div class='modal-dialog' role='document' style='width:1000px'>
    <div class='modal-content' style='width:1000px'>
      <div class='modal-header' style='width:1000px'>
        <button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>
        <h4 class='modal-title' id='myModalLabel'>" + nome+ @"</h4>
      </div>
      <div class='modal-body' style='width:1000px'>


<pre><code>
" + this.formata(funcaoLimpa)+ @"
        </code></pre>
        
" + codeList(cabecalhoLimpo, "list") + @"

      </div>
      <div class='modal-footer' style='width:1000px'>
        <button type='button' class='btn btn-danger' data-dismiss='modal'>Close</button>
        <button type='button' class='btn btn-info' onClick=" + "\"javascript:hyper('"+arquivoOriginal.Name+".html');void(0);\">Visualizar HyperCode do arquivo</button>"+@"
      </div>
    </div>
  </div>
</div>

";



            }

            //blocos
            Regex padraoBlocos = new Regex(@"/\*\*\*(\n|\s)*\[init block\](.|\n)*?\[end block\]\s*\*\*\*/", RegexOptions.None);
            foreach (Match blocos in padraoBlocos.Matches(arquivo))
            {

                Regex retira = new Regex(@"/\*\*\*(\n|\s)*\[init block\](.|\n)*?\*\*\*/|/\*\*\*(\n|\s)*\[end block\](.|\n)*?\*\*\*/", RegexOptions.None);
                string blocoLimpo = retira.Replace(blocos.Value, "");

                //pega o cabeçalho da bloco
                Regex cabecalho = new Regex(@"/\*\*\*(\n|\s)*\[init block\](.|\n)*?\*\*\*/", RegexOptions.None);

                foreach (Match blocoCabecalho in cabecalho.Matches(blocos.Value))
                {
                    //funcaoCabecalho.Value detem o cabeçalho agora
                    //temos que limpar o cabeçalho
                    Regex limpaBloco = new Regex(@"/\*\*\*(\n|\s)*\[init block\](\n|\s)*|(\n|\s)*\*\*\*/", RegexOptions.None);
                    cabecalhoLimpo = limpaBloco.Replace(blocoCabecalho.Value, "");
                    //cabecalhoLimpo detem o cabeçalho sem os comandos cdoc

                }
                /**
                 * blocoLimpo detem o bloco
                 * cabecalhoLimpo detem o bloco
                 * guardar em arrays para se usar no index das funções depois
                 * **/

                // ---------------------------------------------------------------------------------------------------
                // tem que tratar o cabeçalho para se extrair o nome e a descrição da função
                // ---------------------------------------------------------------------------------------------------

                String r = @":|\n";
                // divide o arquivo se encontrar ':' ou '\n'
                // se o code[i] for igual a 'nome', code[i+1] será 'kaue'

                String[] codes = Regex.Split(cabecalhoLimpo, r);
                string nome = "";
                string desc = "";

                for (int k = 0; k < codes.Length; k++)
                {

                    switch (codes[k].Trim().ToLower())
                    {


                        case "#name":
                            nome = codes[k + 1];
                            break;

                        case "#desc":
                            desc = codes[k + 1];
                            break;
                    }
                }

                // ---------------------------------------------------------------------------------------------------
                // agora temos nome e desc
                // ---------------------------------------------------------------------------------------------------

                contadorDeModals++;
                list_meio += @"

                <div class='list-group' data-toggle='modal' data-target='#"+contadorDeModals+@"'>
                    <a href = '#' class='list-group-item '>
                        <h4 class='list-group-item-heading'>
                            " + arquivoOriginal.Name + " > " + nome + @"
                        </h4>
                        <p class='list-group-item-text'>
                            " + desc + @"
                        </p>
                    </a>
                </div>

                ";

                // cria o modal
                html_fim += @"

<!-- Modal Lista-->
<div class='modal fade' id='" + contadorDeModals + @"' tabindex='-1' role='dialog' aria-labelledby='myModalLabel'>
  <div class='modal-dialog' role='document' style='width:1000px'>
    <div class='modal-content' style='width:1000px'>
      <div class='modal-header' style='width:1000px'>
        <button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>
        <h4 class='modal-title' id='myModalLabel'>" + nome + @"</h4>
      </div>
      <div class='modal-body' style='width:1000px'>


<pre><code>
" + this.formata(blocoLimpo) + @"
        </code></pre>
        
" + codeList(cabecalhoLimpo, "list") + @"

      </div>
      <div class='modal-footer' style='width:1000px'>
        <button type='button' class='btn btn-danger' data-dismiss='modal'>Close</button>
        <button type='button' class='btn btn-info' onClick=" + "\"javascript:hyper('" + arquivoOriginal.Name + ".html');void(0);\">Visualizar HyperCode do arquivo</button>" + @"
      </div>
    </div>
  </div>
</div>

";


            }

            //Console.WriteLine(list_meio);
        }


        // essa função retira as linhas em brancos e transforma tags em html entites
        public string formata(string x)
        {
            string res = "";
            String p = "\n";
            String[] elements = Regex.Split(x, p);

             Regex r = new Regex(@"^\r",RegexOptions.IgnoreCase);


            for (int i = 0; i < elements.Length; i++)
            {

                if (!r.IsMatch(elements[i]))
                {
                    res += WebUtility.HtmlEncode(elements[i]);
                }
            }
                
            

            return res;
        }

        public void ini()
        {

            //carrega o arquivo
            FileStream fs = new FileStream(@"D:\teste.js", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            string conteudo = sr.ReadToEnd();
            sr.Close();
            //-- fim função arquivo

            Console.WriteLine("Teste de Expressões Regulares");

            foreach (Match cdoc in padrao.Matches(conteudo))
            {
                string resultado = padraoTroca.Replace(cdoc.Value, "");
                //Console.WriteLine(cdoc.Value+"  \n\n\n");
                //no primeiro teste tem que verificar se o arquivo começa com #cdoc
                //esta dentro do resultado
                //o resultado deve ser analizado
                Regex inicio = new Regex("^(\n|\\s)*#cdoc", RegexOptions.None);
                if (inicio.IsMatch(resultado))
                {
                    Console.WriteLine("\n\tInicio válido\n\n");
                    blocoInicio = resultado;
                    /** registro o bloco inicial dentro da variável resultado **/

                    //tenho que registrar todo o resto do arquivo em outra variável para ser analizada
                    Regex tiraInicio = new Regex(@"^/\*\*\*(.|\n)*?\*\*\*/", RegexOptions.None);
                    // o '^' significa a correspondencia no inicio da expressão.
                    string arquivo = tiraInicio.Replace(conteudo, "");

                   //Console.WriteLine("\n\n"+resultado);


                    //sera dentro desta primeira verificação que todo o codigo se desenrolará
                    //agora tenho o cabeçalho dentro da variavel blocoInicio 
                    //e tenho todo o resto do arquivo a ser analizado dentro da variavel arquivo
                    


                    //#########################################################################################

                    //RETRATAMENTO DA VARIÁVEL ARQUIVO

                    //[ INIT ]
                    
                    //pegas as funções 
                    Regex padraoFuncoes = new Regex(@"/\*\*\*(\n|\s)*\[init function\](.|\n)*?\[end function\]\s*\*\*\*/", RegexOptions.None);
                    foreach (Match funcoes in padraoFuncoes.Matches(arquivo))
                    {
                        //funcoes guarda todas as funções do arquivo
                        //guardar em array ?
                        //não há problema em pegar a função dentro do bloco desde que sejam aplicadas as tags corretas

                        //retira os comandos cdoc da variavel funcoes
                        //var funcaoLimpa retem a função sem comentarios
                        Regex retira = new Regex(@"/\*\*\*(\n|\s)*\[init function\](.|\n)*?\*\*\*/|/\*\*\*(\n|\s)*\[end function\](.|\n)*?\*\*\*/", RegexOptions.None);
                        string funcaoLimpa = retira.Replace(funcoes.Value, "");
                        

                        //pega o cabeçalho da função
                        Regex cabecalho = new Regex(@"/\*\*\*(\n|\s)*\[init function\](.|\n)*?\*\*\*/", RegexOptions.None);
                        
                        foreach (Match funcaoCabecalho in cabecalho.Matches(funcoes.Value))
                        {
                            //funcaoCabecalho.Value detem o cabeçalho agora
                            //temos que limpar o cabeçalho
                            Regex limpaCabecalho = new Regex(@"/\*\*\*(\n|\s)*\[init function\](\n|\s)*|(\n|\s)*\*\*\*/", RegexOptions.None);
                            cabecalhoLimpo = limpaCabecalho.Replace(funcaoCabecalho.Value, "");
                            //cabecalhoLimpo detem o cabeçalho sem os comandos cdoc
                            
                        }
                        /** ###
                         * agora eu tenho
                         * funcaoLimpa que retem a funcao em si sem os comando cdoc
                         * cabecalhoLimpo que retem o cabecalho sem os comando cdoc
                         * todo: gerar codigo html
                         * tem que fazer a substituição no arquivo original ou criar uma variavel nova chamada saida
                         * agora tenho cabecalhoLimpo com o cabeçalho da função - que pode ser tratado com split - e tenho 
                         * funcao limpa com o corpo da funcao.
                         * o que deve ser feito depois é recriar o arquivo apenas com substituições e usar essas variaveis para criar o index
                         * das funções
                         * 
                         */


                        

                    }

                    //blocos
                    Regex padraoBlocos = new Regex(@"/\*\*\*(\n|\s)*\[init block\](.|\n)*?\[end block\]\s*\*\*\*/", RegexOptions.None);
                    foreach (Match blocos in padraoBlocos.Matches(arquivo))
                    {

                        Regex retira = new Regex(@"/\*\*\*(\n|\s)*\[init block\](.|\n)*?\*\*\*/|/\*\*\*(\n|\s)*\[end block\](.|\n)*?\*\*\*/", RegexOptions.None);
                        string blocoLimpo = retira.Replace(blocos.Value, "");

                        //pega o cabeçalho da bloco
                        Regex cabecalho = new Regex(@"/\*\*\*(\n|\s)*\[init block\](.|\n)*?\*\*\*/", RegexOptions.None);

                        foreach (Match blocoCabecalho in cabecalho.Matches(blocos.Value))
                        {
                            //funcaoCabecalho.Value detem o cabeçalho agora
                            //temos que limpar o cabeçalho
                            Regex limpaBloco = new Regex(@"/\*\*\*(\n|\s)*\[init block\](\n|\s)*|(\n|\s)*\*\*\*/", RegexOptions.None);
                            cabecalhoLimpo = limpaBloco.Replace(blocoCabecalho.Value, "");
                            //cabecalhoLimpo detem o cabeçalho sem os comandos cdoc

                        }
                        /**
                         * blocoLimpo detem o bloco
                         * cabecalhoLimpo detem o bloco
                         * guardar em arrays para se usar no index das funções depois
                         * **/

                       


                    }


                    // ##########################################################################################
                    // TRABALHANDO COM O SPLIT REPLACE
                    // ##########################################################################################
                    

                    // var conteudo retem o arquivo original

                    //vamos fazer um split

                    String p = @"/\*\*\*|\*\*\*/";

                    String[] elements = Regex.Split(conteudo, p);

                    string saida = "";

                    string tmp="";

                    Regex initFunction=new Regex(@"\[init function\]",RegexOptions.None);
                    Regex endFunction=new Regex(@"\[end function\]",RegexOptions.None);
                    Regex initBlock=new Regex(@"\[init block\]",RegexOptions.None);
                    Regex endBlock=new Regex(@"\[end block\]",RegexOptions.None);
                    Regex initBotao = new Regex(@"-->", RegexOptions.None);
                    Regex endBotao = new Regex(@"<--", RegexOptions.None);
                    string a;


                    //Console.WriteLine(quebraDeLinha.Replace("ola\nmundo","-"));
                    //Console.WriteLine("ola\nmundo");



                    for (int i = 0; i < elements.Length; i++)
                    {
                        //Console.WriteLine("evento i: " + i + "\n" + elements[i] + "\n---------------\n\n");
                        //Console.WriteLine(elements.Length);
                        if (i == 1)
                        {
                            //Console.WriteLine(elements[7]);
                            saida += @"<div class='panel panel-warning'><div class='panel-heading'>" + codeList(elements[1], "titulo") +
                                @"</div><div class='panel-body'><pre><code>";
                        }
                        else if (initFunction.IsMatch(elements[i]))
                        {
                            
                            saida += @"</code></pre><div class='panel panel-primary noMargin'><div class='panel-heading'>"+codeList(elements[i],"funcao")+
                                        @"</div><div class='panel-body noMargin'><pre><code>";
                          
                        }
                        else if (endFunction.IsMatch(elements[i]))
                        {
                            saida += @"</code></pre></div></div><pre><code>";
                        }
                        else if (initBlock.IsMatch(elements[i]))
                        {
                            saida += @"</code></pre><div class='panel panel-primary'><div class='panel-body'><pre><code>";
                            tmp = elements[i];
                        }
                        else if (endBlock.IsMatch(elements[i]))
                        {

                           
                            saida += @"</code></pre></div><div class='panel-footer'>"+codeList(tmp,"bloco")+
                                @"</div></div><pre><code>";
                        }
                        else if(initBotao.IsMatch(elements[i]))
                        {

                            a = elements[i].Replace("-->", "");
                            saida += @"</code></pre><button type='button' class='btn btn-default myButton' data-toggle='tooltip' data-placement='right' title='"+a+@"'>";
                        }
                        else if (endBotao.IsMatch(elements[i]))
                        {

                            a = elements[i].Replace("<--", "");
                            saida += @"<span class='glyphicon glyphicon-search' aria-hidden='true'></span></button><pre><code>";
                        }
                        else if (i == (elements.Length - 1))
                        {
                            saida += @"</code></pre></div></div>";
                        }

                        else
                        {
                            saida += elements[i].Trim();
                        }
                    }
                    string inicioHTML = @"<!DOCTYPE html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
    <title>#CDOC : teste.js</title>
    <script src='js/jquery.js'></script>
    
    <link href='css/bootstrap.min.css' rel='stylesheet'>
    <script src='js/bootstrap.js'></script>
    <link rel='stylesheet' href='highlight/styles/color-brewer.css'>
    <script src='highlight/highlight.pack.js'></script>
    <script>
    hljs.initHighlightingOnLoad();
    $(function () {
    $('[data-toggle="+'\u0022'+"tooltip"+'\u0022'+ @"]').tooltip()
    })
    </script>
    
    <style type='text/css'>
    .myButton {
        margin-left:20px;
    }
    pre{
    background-color: transparent;
    padding: 0;
    margin: 0;
    border:0;
    }
    .noMargin {margin:0;padding:0;}
    .btn, .btn-default{
    font-family: Courier New;
    font-size: 12px;
    font-weight: bold;
    color:#0000cc;
    }
    .tooltip-inner{
    background-color: #ddd;
    color: #444;
    }
    .noCode{
    color:red;
    }
    .table2 {
    font-size:10px;
    color:#555;
    margin:0;
    }
    .table3 {
    font-size:10px;
    color:#eee;
 margin:0;

    }
    </style>
    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
    <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
    <!--[if lt IE 9]>
    <script src='https://oss.maxcdn.com/html5shiv/3.7.2/html5shiv.min.js'></script>
    <script src='https://oss.maxcdn.com/respond/1.4.2/respond.min.js'></script>
    <![endif]-->
  </head>
  <body>
    
    <div class='container'>
      <div class='page-header'>
        <h1>#CDOC <small>Cedros Documenter </small></h1>
      </div>
      <div class='alert alert-success' role='alert'>File: teste.js</div>";
                    string meioHTML = saida;
                    string fimHTML = @" </body></html>";

                    FileInfo arq = new FileInfo(@"D:\PROGRAMACAO\VISUAL STUDIO\cdoc\htmlCdoc\testeBin.html");
                    if (File.Exists(arq.FullName))
                    {
                        arq.Delete();
                    }

                    fs = new FileStream(@"D:\PROGRAMACAO\VISUAL STUDIO\cdoc\htmlCdoc\testeBin.html", FileMode.OpenOrCreate, FileAccess.Write);
                
                StreamWriter sw = new StreamWriter(fs);
                    sw.Write(inicioHTML+meioHTML+fimHTML);
                    sw.Close();
                    







                    Console.WriteLine("Documentação gerada com sucesso");

                    //##########################################################################################

                  
                    Console.ReadLine();
                    return;
                }
                else
                {
                    Console.WriteLine("\nInicio Inválido");
                    Console.ReadLine();
                    return;                   
                }

            }

            Console.WriteLine("\n\n\nPressione Enter para sair");
            Console.ReadLine();
        }

        public string codeList(string x, string y)
        {

            String r = @":|\n";
            // divide o arquivo se encontrar ':' ou '\n'
            // se o code[i] for igual a 'nome', code[i+1] será 'kaue'

            String[] codes = Regex.Split(x, r);

            string titulo = "";

            string cab = "";

            int w = 100;//width da primeira <td>

            if (y == "titulo")
            {
                cab += "<table class=\"table table-hover\">"; w = 250;
            }
            if (y == "bloco")
            {
                cab += "<table class=\"table table-condensed table2\">"; w = 100;
            }
            if (y == "funcao")
            {
                cab += "<table class=\"table table-condensed table3\">"; w = 100;
            }
            if (y == "list")
            {
                cab += "<table class=\"table table-condensed\">"; w = 250;
            }
            if (y == "projeto")
            {
                cab += @"<table class='table table-hover'>
                        <thead><tr><td><b><i>Propriedade</i></b></td><td><b><i>Valor</i></b></td></tr></thead>"; w = 200;
            }
          
            for (int k = 0; k < codes.Length; k++)
            {
                

                switch (codes[k].Trim().ToLower())
                {


                    case "#autor":
                        cab += "<tr><td width='"+w+"px'>Autor</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#init":
                        cab += "<tr><td width='" + w + "px'>Inicio do Desenvolvimento</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#lc":
                        cab += "<tr><td width='" + w + "px'>Última Modificação</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#project":
                        cab += "<tr><td width='" + w + "px'>Integrante do Projeto</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#title":
                        cab += "<tr><td width='" + w + "px'>Título</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#filename":
                        cab += "<tr><td width='" + w + "px'>Nome do Arquivo</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#dependences":
                        cab += "<tr><td width='" + w + "px'>Dependências para Funcionar</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#desc":
                        cab += "<tr><td width='" + w + "px'>Descrição</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#obs":
                        cab += "<tr><td width='" + w + "px'>Observações</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#version":
                        cab += "<tr><td width='" + w + "px'>Versão</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";
                        break;

                    case "#name":
                        titulo += "<h3>" + this.formata(codes[k + 1]) + "</h3>";
                        cab += "<tr><td>Nome</td><td>" + this.formata(codes[k + 1]) + "</td></tr>";

                        if (y == "projeto_nome")
                        {
                            hasName = true;
                            return this.formata(codes[k + 1]);
                        }

                        break;


                }
            }
            cab += "</table>";

            return titulo+cab;
        }
    }

}
