# Segata Sakura — publier le logiciel sur GitHub et le site sur NekoWeb

Deux archives indépendantes :
- segata-sakura-logiciel-github.zip : sources du logiciel uniquement.
- segata-sakura-nekoweb.zip : site prêt à importer, index.html directement à la racine.

## 1. Créer le dépôt du logiciel
Connectez-vous à https://github.com/new avec TaigaMasuku. Choisissez le nom segata-sakura. Choisissez Public pour partager les sources, ou Private pendant la préparation. Ne cochez pas l’ajout automatique d’un README, .gitignore ou licence : les fichiers de départ sont fournis. Cliquez sur Create repository.
Adresse attendue après création : https://github.com/TaigaMasuku/segata-sakura
Ce dépôt n’a pas été créé automatiquement.

## 2. Envoyer le code avec GitHub Desktop
1. Installez GitHub Desktop depuis https://desktop.github.com/download/ et connectez-vous.
2. File > Clone repository : sélectionnez TaigaMasuku/segata-sakura et un dossier local vide.
3. Décompressez segata-sakura-logiciel-github.zip ailleurs.
4. Copiez tout son contenu dans le dossier cloné, y compris .gitignore. À la racine doivent figurer README.md, app/ et release-notes/, pas un dossier supplémentaire.
5. Dans GitHub Desktop, vérifiez les fichiers ajoutés. Aucun jeu, BIOS, réglage personnel ni installateur ne doit apparaître.
6. Dans Summary, saisissez « Première version de Segata Sakura », puis Commit to main.
7. Cliquez sur Publish branch ou Push origin. Ouvrez votre dépôt sur GitHub pour vérifier les fichiers.
Le dossier app/src contient le code C#. BUILD-INSTALLER.md explique la compilation et les dépendances. Ce paquet contient notre interface et son intégration ; le moteur Kronos reste un composant tiers inchangé.

## 3. Publier l’installateur dans Releases
L’installateur dépasse la limite de 100 Mio des fichiers Git ordinaires : ne l’envoyez pas dans le code.
1. Dans le dépôt GitHub : Releases > Draft a new release.
2. Créez le tag v0.2.0-beta.1 sur main, titre « Segata Sakura 0.2.0 — Bêta Windows ».
3. Copiez la description fournie dans release-notes/v0.2.0-beta.1.md.
4. Joignez SegataSakura-Setup-0.2.0-x64.exe et SHA256.txt, disponibles dans le dossier installateur fourni séparément.
5. Cochez This is a pre-release. Save draft permet de conserver un brouillon.
6. Après validation des tests, notices et points de redistribution documentés, Publish release rend la release accessible selon la visibilité du dépôt. Pour un téléchargement public sur le site, utilisez un dépôt public.
Les crédits ne remplacent pas les licences ou l’autorisation concernant le logo. Les limites connues restent dans app/NOTICES.txt et CREDITS.md.

## 4. Importer le site sur NekoWeb
1. Dans le gestionnaire, cliquez Open sur segata-sakura.nekoweb.org/ (le dossier de votre capture).
2. S’il contient déjà des fichiers, Export ZIP permet de les sauvegarder : les fichiers de même nom seront remplacés.
3. À l’intérieur de ce dossier, cliquez Import ZIP et choisissez segata-sakura-nekoweb.zip.
4. Vérifiez que index.html, style.css, config.js et assets/ sont directement dans ce dossier.
5. Ouvrez https://segata-sakura.nekoweb.org/ et testez les pages. Ctrl + F5 recharge les fichiers si nécessaire.
L’adresse est reprise exactement de votre capture ; l’archive ne crée ni domaine ni hébergement. Aucune publication NekoWeb n’a été effectuée depuis cette conversation.

## 5. Activer le téléchargement sur le site
Après la publication effective de la release, éditez config.js dans le dossier du site NekoWeb :

```js
window.SEGATA_CONFIG = {
  account: "TaigaMasuku",
  repository: "TaigaMasuku/segata-sakura",
  tag: "v0.2.0-beta.1",
  asset: "SegataSakura-Setup-0.2.0-x64.exe",
  published: true
};
```

Enregistrez sur NekoWeb. Le tag et le nom de fichier doivent correspondre exactement à la release. Testez le téléchargement en navigation privée. En attendant, le site affiche « Release à venir » et renvoie vers votre profil GitHub.

## 6. Mettre à jour
Logiciel : modifiez les sources, compilez et testez, puis Commit et Push dans GitHub Desktop. Créez une nouvelle release pour chaque version, avec le nouvel installateur et son SHA256.
Site : modifiez les fichiers localement puis importez le ZIP mis à jour sur NekoWeb, ou éditez les fichiers dans son gestionnaire. Mettez à jour config.js à chaque nouvelle release. Aucun GitHub Pages n’est nécessaire.

## Documentation
- https://docs.github.com/en/migrations/importing-source-code/using-the-command-line-to-import-source-code/adding-locally-hosted-code-to-github
- https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository
- https://docs.nekoweb.org/getting-around/dashboard
