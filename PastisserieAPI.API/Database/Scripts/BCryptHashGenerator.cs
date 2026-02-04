// PROGRAMA AUXILIAR: Ejecutar con dotnet-script o crear proyecto console temporal

/*
INSTRUCCIONES:

1. Crear proyecto console temporal:
   dotnet new console -n BCryptHashGenerator
   cd BCryptHashGenerator
   dotnet add package BCrypt.Net-Next

2. Reemplazar Program.cs con este código

3. Ejecutar:
   dotnet run

4. Copiar el hash generado al script SQL
*/

using System;
using BCrypt.Net;

namespace PastisserieAPI.API.Database.Scripts
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("🔐 GENERADOR DE HASH BCRYPT PARA ADMINISTRADOR");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();

            Console.Write("Ingresa la contraseña generada en el script SQL: ");
            string? password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("❌ Contraseña vacía. Saliendo...");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("⏳ Generando hash BCrypt...");

            string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("✅ HASH GENERADO EXITOSAMENTE");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("📋 Hash BCrypt:");
            Console.WriteLine(hash);
            Console.WriteLine();
            Console.WriteLine("📝 PASOS SIGUIENTES:");
            Console.WriteLine("   1. Copia el hash de arriba");
            Console.WriteLine("   2. Abre el archivo 01_CreateAdminUser.sql");
            Console.WriteLine("   3. Busca la línea: DECLARE @PasswordHash NVARCHAR(500) = 'HASH_BCRYPT_AQUI';");
            Console.WriteLine("   4. Reemplaza HASH_BCRYPT_AQUI con el hash copiado");
            Console.WriteLine("   5. Descomenta la sección (elimina /* y */)");
            Console.WriteLine("   6. Ejecuta el script SQL en SSMS");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");

            Console.WriteLine();
            Console.Write("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}