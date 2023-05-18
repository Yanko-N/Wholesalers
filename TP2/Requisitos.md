# **Checklist**

## **Servidor**
* [x] Autenticação
* [x] Administração
* [x] Database
* [ ] Comunicação
* [ ] Ações

## **Cliente - Administrador**
* [x] Autenticação
* [ ] Ações

## **Cliente - Operador Externo**
* [x] Autenticação
* [ ] Ações
---
# **Ações**
## **Reserva**
1. Enviar a modalidade (velocidades DOWN/UP  ex:100/200) e domicilio
2. Indicar se foi reservado, em caso de sucesso devolver UID
3. Atribuir UID ao operador e domicilio

## **Ativação**
1. Enviar UID
2. Verificar se é para o cliente e se o domicilio está RESERVED ou DEACTIVATED
3. Resposta **sincrona** se é possivel e ET
4. Cliente subscrever *"EVENTS"*
5. Server publicar nos "EVENTS" quando ativo (usar sleep pra simular)

## **Desativação**
1. Enviar UID
2. Verificar se é para o cliente e se o domicilio está ACTIVE
3. Resposta **sincrona** se é possivel e ET
4. Cliente subscrever *"EVENTS"*
5. Server publicar nos "EVENTS" quando desativar (usar sleep pra simular)

## **Terminação**
1. Enviar UID
2. Verificar se é para o cliente e se o domicilio está DEACTIVATED
3. Resposta **sincrona** se é possivel e ET
4. Cliente subscrever *"EVENTS"*
5. Server publicar nos "EVENTS" quando desativar (usar sleep pra simular)
6. Apagar domicilio da DB (desativar -> adicionar coluna de bool)

---

# **Servidor**

## **Autenticação**
* Username
* Password (Salted hash using Bcrypt)
* RPC

## **Administração**
* Gerir as coberturas disponíveis para cada
operador.
* Listar todos os serviços RESERVADOS ACTIVOS E TERMINADOS

## **Database**
* Logs de todos os eventos realizados
* Tabela de users


## **Comunicação**
* **Sync** - RPC
* **Async** - RabbitMQ 


---

# **Cliente - Administrador**
## **Autenticação**
* Username
* Password

## **Ações**
* Listagem da cobertura atual
* Listar Eventos

---
# **Cliente  - Operador Externo**

## **Autenticação**
* Username
* Password

## **Ações**
* Realizar ações
* Subscrever tópico



