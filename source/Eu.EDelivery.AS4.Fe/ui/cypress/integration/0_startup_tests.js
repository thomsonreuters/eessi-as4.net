describe('initial setup tests', () => {
    before('setup environment fixtures', () => cy.resetenv())

    it('setup page', () => {
        cy.visit('/')
        cy.fixture('login').then((json) => {
            cy.getdatacy('adminPassword').type(json.password)
            cy.getdatacy('readonlyPassword').type(json.password)
            cy.getdatacy('generateKey').click()
            cy.getdatacy('submit').click()
            cy.getdatacy('header').should('contain', 'Setup is finished!')
        })
    })

    it('continue to settings', () => {
        cy.getdatacy('continueToMain').click()
        cy.url().should('contain', '/login')
    })

    it('returns to login after logout', () => {
        cy.login()
        cy.visit('/settings/portal')
        cy.getdatacy('logout').click()
        cy.url().should('contain', '/login')
    })
})