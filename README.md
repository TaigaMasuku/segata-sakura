# Segata Sakura

Interface Windows pour une bibliothèque de jeux Sega Saturn, utilisant Kronos comme moteur externe embarqué dans l'installateur.

**Interface modifiée et clarifiée, intégration et présentation par Taiga Masuku, alias Théo M.** Le cœur et le binaire Kronos 2.7.0 ne sont pas modifiés par cette version.

État : bêta 0.2.0. Les tests de bibliothèque et les essais isolés d'installation passent ; les parties réelles, audio, manettes et l'installation sur un Windows vierge restent à valider. Aucun jeu ni BIOS n'est fourni.

## Dossiers

- `app/src` : sources C# de l'interface et du sélecteur Windows.
- `app/build.ps1` : compilation avec .NET Framework installé sur Windows.
- `app/installer.iss` : script Inno Setup.
- `release-notes` : texte préparé pour la première release.
- `CREDITS.md` et `app/NOTICES.txt` : auteurs, dépendances et limites de redistribution.

## Compiler l'interface

Depuis PowerShell, à la racine du dépôt :

```powershell
powershell -ExecutionPolicy Bypass -File .\app\build.ps1
```

Le résultat est `app/SegataSakura-0.2.exe`. Le script de build est lisible et ne télécharge rien. Les icônes nécessaires sont incluses.

Les binaires Kronos et ses DLL ne sont pas versionnés dans Git. Pour exécuter localement la bibliothèque avec le moteur, placer le paquet Windows complet de Kronos 2.7.0 dans `app/engine`, avec `kronos.exe`, ses DLL et ses dossiers de plugins. Ne copier ni BIOS ni jeu dans ce dépôt.

L'installateur inclut le moteur, contrairement au dépôt source. Pour recompiler l'installateur, lire `BUILD-INSTALLER.md`.

## Site

Le site est hébergé séparément sur NekoWeb à l’adresse prévue https://segata-sakura.nekoweb.org/. Son code ne fait pas partie de ce dépôt. Voir `TUTORIEL-GITHUB.md` pour publier les sources du logiciel.

## Distribuer une bêta

Joindre l'EXE d'installation et SHA256.txt à une GitHub Release, pas à un commit. Le paquet contient les sources tierces disponibles. Conserver la release en brouillon jusqu'à clarification des points de droits et de provenance décrits dans les notices.

## Licences

Kronos et ses composants conservent leurs licences. Ce dépôt ne prétend pas relicencier leurs sources ou le logo Sakura. La licence de réutilisation de l'interface originale reste à décider par son auteur avant de présenter le projet comme open source. Mettre des sources sur GitHub ne définit pas à lui seul une licence de réutilisation.

Projet de fans indépendant, sans affiliation officielle avec Sega.
