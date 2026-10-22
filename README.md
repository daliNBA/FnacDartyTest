Livrable attendu : archive ZIP contenant le projet complet (code source, tests, scripts SQL, documentation, etc.)

### 1\) Corriger le(s) bug(s)

Durée estimée : 15 minutes

Lancez l'application puis tentez d'ajouter un nouveau livre avec la requête suivante :
POST /api/library/addBook { "title": "Guerre et paix", "author": "Leon Tolstoï" }

Constat : la requête retourne une erreur.
Attendu : la requête doit aboutir et le livre doit être ajouté.

Identifiez la (ou les) cause(s) de l'erreur et appliquez la (ou les) correction(s) nécessaire(s) dans le code.

### 2\) Implémenter une nouvelle fonctionnalité : emprunt de livres

Durée estimée : 45 minutes

L'objectif est d'implémenter un nouvel endpoint permettant de traiter l'emprunt d'un ou plusieurs livres, en appliquant des règles simples de disponibilité et de refus.

L'API doit :

* traiter la demande d'emprunt pour le client fourni
* déterminer quels livres sont empruntables et lesquels ne le sont pas
* créer un emprunt uniquement par livre effectivement emprunté
* retourner un résultat permettant à l'utilisateur de comprendre :

  * quels livres ont été empruntés et leur date de retour prévue
  * quels livres ont été refusés et la raison du refus

Exemple de requête :
POST /api/library/borrow
{
"customerId": 4,
"bookIds": \[1, 3, 5, 6]
}

Exemple de réponse :
{
"borrowedBooks": \[
{ "bookId": 1, "dueAt": "2026-02-01" },
{ "bookId": 2, "dueAt": "2026-02-01" },
{ "bookId": 5, "dueAt": "2026-02-01" }
],
"rejectedBooks": \[
{ "bookId": 8, "reasonCode": "LIMIT\_REACHED", "reasonLabel": "Le client a atteint la limite d'emprunts simultanés" }
]
}

Disponibilité :

* Un livre est empruntable s'il n'existe aucun emprunt actif sur ce livre.
* Un emprunt est actif si la date de retour effective n'est pas renseignée, même si la date de retour prévue est dépassée.

Création d'un emprunt :

* Un emprunt est créé uniquement pour les livres empruntables.
* La durée d'un emprunt est fixée à 21 jours à partir de la date du jour.

Motif de refus :

* Pour chaque livre non empruntable, un code et libellé de raison de refus doivent être fournis parmi les suivants et dans cet ordre de priorité :

  * NOT\_FOUND : "Le livre est introuvable"
  * NOT\_AVAILABLE : "Le livre n'est pas disponible actuellement"
  * LOAN\_OVERDUE : "La date d'échéance d'un prêt est dépassé"
  * LIMIT\_REACHED : "Le client a atteint la limite de 3 emprunts simultanés"

Codes de retour HTTP :

* Si au moins un livre demandé est empruntable, l'API retourne un code HTTP 200 OK avec le résultat décrit ci-dessus.
* Si le client n'existe pas, l'API retourne un code HTTP 404 NotFound avec le message "Le client est introuvable."
* Si la requête n'est pas formatée correctement, l'API retourne un code HTTP 400 BadRequest avec un message adapté.
* Si une erreur inattendue survient, l'API retourne un code HTTP 500 InternalServerError avec le message "Une erreur inattendue est survenue, veuillez réessayer ultérieurement.". Le détail de l'erreur doit être logué côté serveur mais ne doit pas être exposé au client.

Remarques :

* Même si aucun livre n'est emprunté (tous rejetés), la réponse reste 200 OK dès lors que l'entrée est valide et que le client existe, et la liste rejectedBooks explique pourquoi.
* Afin de simplifier l’exercice, on considère qu’un emprunt correspond à l’association d’un client et d’un livre.
* La notion de "date du jour" correspond à la date côté serveur au moment du traitement.

### 3\) Implémenter les tests unitaires

Durée estimée : 45 minutes

L’objectif est de tester unitairement la logique d’emprunt implémentée à l’étape précédente.







FeedBack

\### Améliorations possibles

\- Ajouter `GetById(int id)` au repository client : aujourd'hui `Get()` renvoie tous les clients

&#x20; et le filtrage se fait en mémoire, ce qui ne passerait pas à l'échelle avec une vraie base.

\### Injection de dépendances

\- Les repositories en mémoire sont enregistrés en \*\*Singleton\*\*. En `Scoped`, les données étaient

&#x20; recréées à chaque requête et les emprunts étaient perdus.

\- Le service est enregistré en \*\*Scoped\*\*.

