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
* [ ] Não deverão processar-se dois ficheiros iguais (Hashing Ficheiros)
* [ ] Usar uma DB (MySql?? / Sqlite??)
* [ ] Classificação por município
* [ ] Linha do ficheiro processada -> atualizar  informação município associado
* [ ] Atendimento concorrente (Threads e Mutexes)

---

# **Protocolo**  

## Cliente
1. IP Server
2. HELLO
3. Input File
4. Get FeedBack
5. QUIT 
6. BYE

## Server
1. HELLO -> 100 OK
2. ACK File 
3. Send FeedBack
4. QUIT -> 400 BYE
5. Close Conection 

### Files 
 + 200 OK
 + 999 Wrong Format (!= .csv)
 + 888 Unknow Encoding 
### Status  
 * OPEN
 * ERROR
 * IN_PROGRESS
 * COMPLETED
 ---