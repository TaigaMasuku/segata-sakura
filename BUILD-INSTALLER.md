# Recompiler l'installateur

Le dépôt contient le code et les scripts, mais pas les gros binaires. Il faut Windows x64, le compilateur .NET Framework fourni par Windows et Inno Setup 6 depuis son site officiel.

1. Compiler `app/build.ps1`.
2. Placer le paquet Kronos 2.7.0 complet dans `app/engine`, y compris son `vc_redist.x64.exe`. Le redistribuable doit être signé par Microsoft.
3. Préparer `app/sources` avec les archives suivantes :
   - `kronos-2.7.0-sources.zip`, tag `2.7.0_official_release`, commit `58352d6dc969fa90c5fa1220f38ffe577157547f` depuis https://github.com/FCare/Kronos ;
   - `qtbase-5.14.2.tar.xz`, `qtsvg-5.14.2.tar.xz`, `qtmultimedia-5.14.2.tar.xz`, `qtimageformats-5.14.2.tar.xz`, `qttranslations-5.14.2.tar.xz`, renommés depuis les archives `MODULE-everywhere-src-5.14.2.tar.xz` de https://download.qt.io/archive/qt/5.14/5.14.2/submodules/ ;
   - `SDL2-2.30.10.zip` depuis https://github.com/libsdl-org/SDL/releases/tag/release-2.30.10.
4. Ouvrir `app/installer.iss` avec Inno Setup et compiler. Le résultat apparaît dans `installateur`.
5. Calculer une nouvelle empreinte SHA-256 de l'EXE final. Ne réutiliser aucune ancienne empreinte après recompilation.

Les sources doivent correspondre aux binaires effectivement distribués : les versions listées décrivent le paquet actuel mais ne constituent pas une vérification reproductible de son origine. Relire les notices avant diffusion.

Le test technique `/SMOKETEST=1` évite registre, raccourcis et redistribuable dans l'environnement d'essai. Il ne remplace pas un essai d'installation normale sur un Windows vierge. Ne pas recommander ce paramètre aux utilisateurs finaux.
