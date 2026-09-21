# Architecture

## Description
The 5D-I-HCDT platform consists of three main parts:

physical entities - physical production lines with a control system (PLC),
clients - mixed reality devices further extending physical entities in the virtual environment,
server - the "single source of truth" for communication between physical entities and clients.

## Scheme
<img width="1417" height="832" alt="image" src="https://github.com/user-attachments/assets/d47b41b7-bc99-433d-b038-07a8c266b27e" />

---

# EXPI40 Server

## Description
The EXPI40 Server represents the "single source of truth" for communication between physical entities and clients. The proposed architecture defines the technologies and their interconnection and mutual interactions for the purpose of creating a 5D-I-DT. Within the EXPI40 Server, the following are deployed:
- **Server-side Unity Application** (S-SUA) - the "single source of truth" for all virtual production lines. It ensures communication with the physical entity, processing of requests from clients, and their subsequent notification.
- **Production Line AAS** - digital descriptions of production lines in the form of AAS.
  - Concept Descriptions - internal CDs, defining semantic interoperability for parts of the AAS without an ECLASS SemanticId.
- **EXPI40 Service Network** - a network of services within the concept of a service-oriented architecture, accessing data in the production process and ensuring optimization, maintenance, and insights.
  - BaSyx Service - ensures access to information contained in the AAS.
  - Data Storages - data storages of various types, e.g. SQL/NoSQL/Time-series databases.
- OPC UA Aggr. Server - aggregates available OPC UA Servers.
  - Discovery Service - ensures automatic discovery of new OPC UA Servers on the network.
- MQTT Broker - a broker serving communication using the PubSub model between PLCs and EXPI40 clients, as well as between the EXPI40 server and EXPI40 clients.

## Scheme
<img width="1818" height="1139" alt="image" src="https://github.com/user-attachments/assets/90b3bb92-3357-4e6b-82cc-f40de82467d6" />

---

# Server-side Unity Application

## Description
The "single source of truth" for all virtual production lines. To create a virtual entity, it accesses data in the Production Line AAS via the BaSyx Service. It ensures communication with the physical entity and updates the co-simulated production in the virtual environment according to information from the physical control system (PLC). It has access to the EXPI40 Service Network for the purposes of optimization and further decision-making.

## Scheme
<img width="1809" height="971" alt="image" src="https://github.com/user-attachments/assets/ad28cc38-230d-44dc-8b14-056e1cf2254a" />

---

# EXPI40 Client

## Description
Clients of the 5D-I-HCDT platform represent augmented reality (AR) and mixed reality (MR) devices that enable co-simulation of physical production and its potential extension in virtual space. Clients take the definition of the visualized production line from the digital description of the line (AAS). Clients submit requests for processing to the control system (PLC) indirectly via the S-SUA (the "single source of truth" for all EXPI40 clients) using the publisher-subscriber model. Clients are subsequently notified of actions performed by the control system, to which they react, e.g. when starting a conveyor. EXPI40 clients also have mediated access to calling EXPI40 services to obtain insights enabling better decision-making.

## Scheme
<img width="1413" height="965" alt="image" src="https://github.com/user-attachments/assets/cc2c98c9-9e86-49e3-b30b-40d3da6823fc" />
