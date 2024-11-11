using System;
using System.Collections.Generic;

namespace CompiladorPASO
{
    class Program
    {
        static void Main(string[] args)
        {
            // Aquí defines el código de prueba que quieres que el lexer procese.
            string codigoFuente = @"
                a = 3 ~primer comentario
                si (a == 3 Y a != 2)
                    muestra ""Hola mundo""
                sino
                    muestra !
                ";

            // Inicializa el lexer con el código fuente.
            Lexer lexer = new Lexer(codigoFuente);

            // Llama a ObtenerTokens() para obtener los tokens generados por el lexer.
            List<Token> tokens = lexer.ObtenerTokens();

            // Imprime los tokens en la consola.
            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }
    }
}
