# Owerall

### The name of the solution is Next Chat

Next Chat is a prove of concept of chat server application. Tiny client is also included.

The language of the solution is C# and platform is Net 5.0.

The delivery form is the Helm chart. 

Redis and MySql is required dependencies. Included in the chart.

## Prerequisites

You need a kubectl and helm tools configured on the host you perform a deploy from. 

# Contents

|Folder|Purpose|
|------|-------|
|.\Tests.Common|Common classes used in tests projects|
|.\Tests.Unit|A set of isolated tests for CI process. These tests do not need any dependency.|
|.\Tests.Integation|A set of tests to run over the existing infrastructure. These tests communicate over Redis and stores a data to MySql. To run tests you need infrastructure components are up and connection strings are in configuration.yaml file.|
|.\Tests.Auto|Black box tests. Those tests should be run over deployed NextServer instance. Also used for helm test.|
|.\Core|Defines models and interfaces.|
|.\NextServer|SignalR server implementation.|
|.\NextClient|Console NextServer client application.|
|.\Services|Library with helper classes to solve use cases.|
|.\SignalR|SignalR client implementation for NextClient and tests.|
|.\Sql|Messages history repository implementation on top of MySql.|
|.\Redis|Redis streams client; groups state persistence layer.|
|.\ Configurator|Application to configure MySql database. Used from Helm hook after the chart deployed.|
|.\Helm| Contains the helm chart.|
|.\\.github\workflows|Contais git hub actions used in CI process|

# Architecture and Approach

## Hi level picture
A solution is based on top of SignalR transport layer. Redis streams is used as messaging layer.
The data required to process messaging is also stored at the Redis side. 
Messages history is stored at the MySql database.

## Server

The server is asp.net core application with SignalR hub. 

### Design
A server uses a SignalR as the communication protocol, Redis streams as messaging layer, Redis as groups persistence layer, and MySql for a messages history. The idea is to keep all required for communication information close to transport layer. Messaging history is moved to separate storage service as far the Redis is not designed to persist large data sets. 

### Authorization
The server trusts any authorization header value and recognizes it as user id. No Bearer or any other prefix is need except when your user name starts from Bearer.

### Scaling
By default, the only instance of the server deployed. However, the only user session is the server state so it can be esialy scaled. The session persistence is pretty common problem and the solution has name of sticky sessions. Sticky sessions configuration depends on the ingress controller type you use in your infrastructure. 
If we go with the one of the msot popular solutions Nginx Ingress Controller we need to define an ingress object for the Next Chart kubernetes service. 
Instructions how to [configure sticky sessions on ingress for Nginx ingress controller.](https://kubernetes.github.io/ingress-nginx/examples/affinity/cookie/).
To leverage the number of instances just specify an appropriate value in values.yaml of the Helm chart.

### Reliability 
As an additional reliability option the design of the server allows users to communicate when messages history service is down.

### API
The interface of the hub defined in the 
[.\Core\Server\INextChatServer.cs](.\Core\Server\INextChatServer.cs)

### Endpoint
The default endpoint is http://your-host:5000/next-chat. 
The host name is depend on your deployment configuration and for a local Kubernetes cluster is localhot.

### Cloud endpoint 
The service is deployed in the Azure and can be accessed at http://20.73.35.46:5000/next-chat

## Client
There is a tiny client in the repository.
The client is console application with some help. So just test it. 

### Commands

#### Connect

Performs connect to a Next Chat server. You can't use it after sucessfully connected/

|Parameter Name|Purpose|
|--------------|-------|
|URI|A Next Server URI to connect.|
|User Id|The User name to show in the chat.|

Example:
````
connect http://20.73.35.46:5000/next-chat DarkNext
````

#### List

Lists available groups on the server.

Example:
````
list
````

#### Create

Creates a chat group on the server.

|Parameter Name|Purpose|
|--------------|-------|
|Group Id|A group name to be created.|

Example:
````
create directors-secret-conversation
````

#### Join

Joins the user to a chat group.

|Parameter Name|Purpose|
|--------------|-------|
|Group Id|A group name to join.|

Example:
````
join off-topic
````

#### Leave

Removes the user from a chat group.

|Parameter Name|Purpose|
|--------------|-------|
|Group Id|A group name to leave.|

Example:
````
leave smbdyBithdayDiscussion
````

#### Bye

Exits the application, however you never want to do this. Do you?

Example:
````
bye smbdyBithdayDiscussion
````

## CI
The CI process has build definition, which produces docker images. The build definition is github action and run at the github server when release is created. When images are available Helm chart is can be used to deploy a service to any Kubernetes cluster. Redis and Mysql dependencies will be spinned up with the chart so you don't need to carry about in very basic scenario. Additionaly, a configurator programm will prepare a database if needed. The configurator program is a part of helm chart and triggered after the chart is deployed or upgraded. So the dependencies and database schema are incomes within the chart. To ensure everything is going as expected run a helm test command after the chart is deployed. 

Example:
````
helm test next-chat
````
The helm chart is located in the .\Helm folder.
The build.ps1 powershell script in the root helps you to produce docker images locally.

### Github Actions

#### ci.yml
The action builds and tests the solution then produces artifacts.
The artifacts are a set of docker images are pushed to the dockerhub.io.

#### merge-build.yml 
The action performs the solution build and test. Used to check pull requests.

## Inter cluster communications
Containers within Kubernetes clusters access each other by internal service name (hardcoded).

# TODO:
- Add telemetry.
- Add logging.
- Add resilence (circuit breaking, retries, reconnection).
- Add unsubscribe from group when no user listens for on hub.
- Add input validation (Filed sizes, group membership, group existence, nulls).
- Check/add re-subscribe to joined groups on reconnect.
- Add PoD crash handling.
- Add friendly client error reporting.
- Make integration tests independent. At this moment tests require clear database and no parallelism.
- Handle multi-server user connection case. At this moment group membership not so fine when a user connected twice.
- Add readiness probe for Kubernetes containers.
- Check group presence at join time.
- Goup join and history send can be merged to single server operation.
- Group membership shold be set with expiration and clients should refresh it periodically.
- Move logic form a NextChatHub class to a dedicated service in core layer.
- Improve tests code coverage.
