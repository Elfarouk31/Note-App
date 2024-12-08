const saveButton = document.querySelector('#btnSave');
const deleteButton = document.querySelector('#btnDelete');

const titleInput = document.querySelector('#title');
const description = document.querySelector('#description');
const noteContainer = document.querySelector('#note__container');

function clearForm()
{
    titleInput.value = '';
    description.value = '';
}

function displayNoteInForm(note)
{
    titleInput.value = note.title; 
    description.value = note.description;
    deleteButton.classList.remove('hidden')
}

function getNoteById(id) {
    fetch(`https://localhost:7021/api/Note/${id}`)
    .then(data => data.json())
    .then(response => displayNoteInForm(response));
}

function populateForm(id)
{
    getNoteById(id);

}

function addNote(title,description)
{
    const body = {
        title : title,
        description : description,
        isVisible: true
    }

    fetch('https://localhost:7021/api/Note', {
        method: 'POST',
        body : JSON.stringify(body),
        headers: {
            "content-type" : "application/json"
        }
    })
    .then(data => data.json())
    .then(response => {
        clearForm();
        GetAllNotes();
    });
}

function GetAllNotes()
{
    fetch('https://localhost:7021/api/Note')
    .then(data => data.json())
    .then(response => displayNotes(response));
}
                        
function displayNotes(notes)
{
    let allNotes = '';

    notes.forEach(note => {
        const noteElement = `
                                <div class="note" data-id="${note.id}">
                                    <h3>${note.title}</h3>
                                    <p>${note.description}</p>
                                </div>
                            `;

        allNotes += noteElement;
    });
    noteContainer.innerHTML = allNotes;

    // add event listner
    document.querySelectorAll('.note').forEach(note => {
        note.addEventListener('click', function() {
            populateForm(note.dataset.id);
        });
    });

}



GetAllNotes()

saveButton.addEventListener('click', function(){
    addNote(titleInput.value, description.value);
})