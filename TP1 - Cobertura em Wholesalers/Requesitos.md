# **SD Grupo 6**
# **TP1 - Cobertura em Wholesalers**

# **Cliente**
* [ ] Receber Input do IP do servidor
* [ ] Interface de texto

---
# **Servidor**
* [ ] Receber ficheiro de cobertura
* [ ] Processar ficheiro de cobertura
* [ ] Armazenar data início do processamento, nome do ficheiro, operador e estado (OPEN, ERROR, IN_PROGRESS, COMPLETED)
* [ ] Não deverão processar-se dois ficheiros iguais (File Checksum [ SHA256? ])
* [ ] Usar uma DB (MySql?? / Sqlite??)
* [ ] Classificação por município
* [ ] Linha do ficheiro processada -> atualizar  informação município associado
* [ ] Atendimento concorrente (Threads e Mutexes)

---

# **Protocolo Cliente-Servidor**  

## Status Codes
+ 100  - OK
+ 101 - COMPLETED

* 200 - IN_PROGRESS
* 201 - OPEN

+ 300 - ERROR

* 400 - BYE


## Cliente
1. IP Server
2. HELLO
3. Input File
4. Get FeedBack
5. QUIT 
6. BYE

## Servidor
1. HELLO -> 100 OK
2. ACK File
3. Send FeedBack
4. QUIT -> 400 BYE
5. Close Conection 

### Ficheiro /  Status Ficheiro
* .CSV

+ 100  - OK
+ 101 - COMPLETED ??

* 200 - IN_PROGRESS ?? 
* 201 - OPEN ?? 

+ 300 - ERROR



 ---

## **Hashing Ficheiros / Checksum**
### ChecksumUtil
```csharp
	public static class ChecksumUtil {
        public static string GetChecksum(HashingAlgoTypes hashingAlgoType, string filename) {
            using (var hasher = System.Security.Cryptography.HashAlgorithm.Create(hashingAlgoType.ToString())) {
                using (var stream = System.IO.File.OpenRead(filename)) {
                    var hash = hasher.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

    }
    public enum HashingAlgoTypes {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

```

### Uso
```csharp
var checksum = ChecksumUtil.GetChecksum(HashingAlgoTypes.SHA256, @"./teste.txt");
```
---
## **CSV Parsing**
### CsvParser
```csharp
public static class CsvParser {
  
        public static List<List<string>> CsvToList(string path, char separator = ',') {
            if (!path.EndsWith(".csv")) throw new Exception("Error: Invalid file type. Please provide a .csv file");
            List<List<string>> rows = new List<List<string>>();
            var buff = File.ReadLines(path);

            foreach (var line in buff) {
                List<string> col = new List<string>();
                var strings = line.Split(separator, StringSplitOptions.TrimEntries);
                foreach (var s in strings) {
                    col.Add(s);
                }
                rows.Add(col);
            }
            return rows;
       }
}
```
### Uso

```csharp
        var lista = CsvParser.CsvToList(@"./teste.csv", ';');
```

---
