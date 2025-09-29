
<a id="readme-top"></a>

<!-- https://www.swisstransfer.com/d/8db686fe-6a49-4744-8099-572cc8e57520 -->

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/MASTTTTT/SAE-5-01-LocURa4IoT-2025-2026.git">
          
  </a>

<h1 align="center">LocURa4IoT</h1>

  <p align="center">
    <br />
    <a href="https://github.com/AchrafAmeri/SAE-5-01-LocURa4IoT-2025-2026/tree/main/Doc"><strong>Explorez la documentation »</strong></a>
    <br />
    <br />
    <a href="https://locura4iot.irit.fr">Demo</a>
    &middot;
    <a href="https://github.com/AchrafAmeri/SAE-5-01-LocURa4IoT-2025-2026/issues">Report Bug</a>
    &middot;
    <a href="https://github.com/AchrafAmeri/SAE-5-01-LocURa4IoT-2025-2026/pulls">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table des matières</summary>
  <ol>
    <li>
      <a href="#a-propos-du-projet">À Propos du projet</a>
      <ul>
        <li><a href="#technologies">Technologies</a></li>
      </ul>
    </li>
    <li>
      <a href="#demarrage">Démarrage</a>
      <ul>
        <li><a href="#prerequis">Prérequis</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## L'équipe de developpement

- [David TRAN](https://github.com/DavidTRANMinhAnh) — Product Owner  
- [Pierre-Louis DUCRY](https://github.com/Ducry-PL) — Scrum Master  
- [Mialisoa RAZAFINIRINA](https://github.com/Mialiso) — Développeuse  
- [Mohammed AMERI](https://github.com/AchrafAmeri/) — Développeur  
- [Matt GUILLUY](https://github.com/MASTTTTT) — Abandon  


## A Propos du projet

LocURa4IoT est une plateforme permettant l’étude de la localisation des objets connectés par les protocoles de communication sans fil, l’analyse de performances des protocoles de ranging (mesure de distance) et de localisation, l’étude et l’analyse des performances des protocoles de synchronisation fine (à la nanoseconde) sur un réseau sans fil. La plateforme permet également l’étude et l’analyse des performances des algorithmes de localisation, ainsi que l’exploitation de la donnée de localisation, et la localisation sémantique. La plateforme est aussi bien utilisable à distance, automatiquement via des scénarios programmés et automatiquement déroulés, que localement, sur place, afin d’impliquer des utilisateurs en personne dans des scénarios d’usage.



<p align="right">(<a href="#readme-top">Aller en haut</a>)</p>



## Technologies 

Le projet LocURa4IoT de l’IRIT repose principalement sur des technologies de communication sans fil dédiées à la localisation d’objets connectés. Les composants essentiels incluent l’Ultra Wide Band (UWB), le Bluetooth Low Energy (BLE), LoRa/LoRaWAN et le NFC, intégrés avec des protocoles avancés de synchronisation, de ranging (mesure de distance), et des architectures réseau centrées autour d’un broker MQTT.

### Protocoles et technologies sans fil

- **UWB (Ultra Wide Band)**
  - Mesure du temps de vol radio (Time of Flight)
  - Angle d'arrivée (Angle of Arrival)
  - Précision de localisation jusqu'à ±2 cm
- **BLE (Bluetooth Low Energy)**
  - Localisation opportuniste dans des environnements variés
- **LoRa / LoRaWAN**
  - Expérimentation sur réseaux longue portée et hétérogénéité IoT
- **NFC**
  - Applications de proximité

### Protocoles, architecture et outils

- **Protocoles de ranging** (mesure de distance) et de localisation
- **Synchronisation fine**
  - Précision jusqu’à 100 nanosecondes
- **Protocoles MAC** (Medium Access Control) et réseaux avancés
- **Automatisation/reproductibilité** des expériences (scripts Python, accès distant)
- **Broker MQTT** (Message Queuing Telemetry Transport)
  - Contrôle, collecte et automatisation des échanges de données

### Infrastructure de la plateforme

- 64 objets connectés UWB (Qorvo DWM1001-dev), entièrement reprogrammables
- 40 contrôleurs testbed sur réseau Ethernet
- Rail mobile de 7 mètres pour les tests de mobilité
- Déploiement sur trois environnements physiques :
  - Bureaux
  - Appartement intelligent (Maison Intelligente de Blagnac)
  - Chambre anéchoïque
---
Plateforme opérée par l’équipe RMESS de l’IRIT, Université Toulouse/Jean Jaurès et IUT de Blagnac


<p align="right">(<a href="#readme-top">Aller en haut</a>)</p>



<!-- GETTING STARTED -->
## Documentation
- [Recueil de besoin](https://github.com/AchrafAmeri/SAE-5-01-LocURa4IoT-2025-2026/blob/main/Doc/Recueil%20de%20besoin.pdf)  
- [Doc utilisateur](https://github.com/AchrafAmeri/SAE-5-01-LocURa4IoT-2025-2026/blob/main/Doc/DocUtilisateur.adoc)  
- [Doc technique](https://github.com/AchrafAmeri/SAE-5-01-LocURa4IoT-2025-2026/blob/main/Doc/DocTechnique.adoc)  
- [Documentation IRIT Lab VR](https://github.com/AchrafAmeri/SAE-5-01-LocURa4IoT-2025-2026/blob/main/Documentation%20IRIT%20Lab%20VR.pdf)  

## Demarrage

IRIT Lab VR est une application de réalité virtuelle qui permet de visualiser les résultats d'expériences menées sur la plateforme LocURa4IoT de l'IRIT. Elle affiche les estimations de position et les mesures de ranging en récupérant les informations publiées sur le réseau MQTT. L'application a été développée avec le moteur de jeu Unity pour les casques MetaQuest.

### Prerequis

L'application est faite pour tourner sur les casques MetaQuest. Ce sont des casques autonomes, ne nécessitant pas d'ordinateur pour fonctionner, ce qui rend l'expérience portable.

### Installation

Le logiciel se présente sous la forme d'un fichier .apk.

Pour l'installer dans votre casque :

- Branchez le casque à votre PC.

- Ouvrez l'application 

- Meta Quest Developer Hub (votre casque doit être en mode développeur).

Dans l'onglet 

- Device Manager, cliquez sur Add Build et ajoutez le fichier .apk.

- Le logiciel sera disponible dans la bibliothèque du casque MetaQuest 3, dans le dossier Unknown Sources

<p align="right">(<a href="#readme-top">Aller en haut</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage

Contrôle du rail : Il est possible de piloter le rail de sept mètres de la plateforme en publiant des requêtes MQTT depuis l'application. La position du chariot se met à jour en temps réel sur la tablette virtuelle et dans la scène 3D.


Affichage des positions des nodes : Les positions réelles et estimées des nodes sont affichées de manière dynamique. Les nodes non connectés sont représentés par des cubes gris, ceux qui sont connectés par des cubes bleu clair, et les positions estimées par des cubes rouges.

État des portes : L'application récupère l'état (ouvert/fermé) des portes via des capteurs, ce qui permet de visualiser leur état en temps réel, car cela peut influencer les mesures.

Représentation des mesures de ranging : Deux types de représentation sont disponibles. Les mesures peuvent être affichées sous forme de lignes droites entre les nodes et les ancres, ou sous forme de sphères dont le centre est l'ancre et le rayon est la distance mesurée.


<p align="right">(<a href="#readme-top">Aller en haut</a>)</p>


<!-- ROADMAP -->
<!-- ## Roadmap


- [ ] Réaliser l'UI pour l'ajout dynamique

Afin d'avoir toutes les features et les [issues](https://github.com/MASTTTTT/SAE-5-01-LocURa4IoT-2025-2026/issues).

<p align="right">(<a href="#readme-top">Aller en haut</a>)</p> -->

## Livrables attendus – Projet VR Locura4IOT

| Catégorie | Livrable | Description / Contenu attendu |
|-----------|----------|--------------------------------|
| **Suivi du projet** | Backlog produit | Liste priorisée des fonctionnalités (user stories, critères d’acceptation). |
| | Backlog sprint | Planification des sprints, tâches sélectionnées, estimation des efforts. |
| | Comptes-rendus (CR) de réunion | Synthèse des décisions, actions, points bloquants, prochaines étapes. |
| | Board du projet | Kanban ou Scrum board pour visualiser l’avancement. |
| **Analyse & conception** | Document d'analyse | Étude du besoin, contraintes, cas d’utilisation, scénarios utilisateurs. |
| | Document de conception | Architecture logicielle, diagrammes (UML, séquence, composants), choix techniques. |
| **Développement** | Code documenté | Code source complet, structuré, commenté et conforme aux bonnes pratiques. |
| **Tests** | Cahier de tests | Jeux d’essais, procédures de tests unitaires et fonctionnels, résultats attendus. |
| | Cahier de recette | Tests de validation, critères d’acceptation avant livraison. |
| **Livraison & clôture** | Revue finale du projet | Présentation de la version finale, bilan des objectifs atteints, retours d’expérience. |
| | Guide d'utilisation | Manuel d’installation, configuration et utilisation de l’application. |


<!-- Sources -->
<!-- ## Sources

* []()
* []()
* []() -->

<p align="right">(<a href="#readme-top">Aller en haut</a>)</p>



