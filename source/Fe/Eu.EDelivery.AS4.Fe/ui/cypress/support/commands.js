// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add("login", (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add("drag", { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add("dismiss", { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This is will overwrite an existing command --
// Cypress.Commands.overwrite("visit", (originalFn, url, options) => { ... })

Cypress.Commands.add('getdatacy', (s) => cy.get(`[data-cy=\"${s}\"]`))

Cypress.Commands.add('login', () => {
    // cy.visit('/login', { timeout: 20000 })
    //     .then((_) => {
    //         cy.fixture('login').then((json) => {
    //             cy.getdatacy('username').type(json.username)
    //             cy.getdatacy('password').type(json.password)
    //             cy.getdatacy('login').click()
    //             cy.url().should('contain', '/settings/portal')
    //         })
    //     })

    cy.fixture('login').then((json) => {
        cy.request({ 
            method: 'POST',
            url: 'api/authentication',
            body: {
                username: json.username,
                password: json.password
            } 
        }).then((resp) => {
            let token = resp.body.access_token;
            localStorage.setItem("id_token", token)
        })
    })
})

Cypress.Commands.add('resetenv', () => {
    cy.log('reset environment')
    cy.exec('.\\cypress\\reset.cmd')
})

Cypress.on('uncaught:exception', (err, runnable) => false)