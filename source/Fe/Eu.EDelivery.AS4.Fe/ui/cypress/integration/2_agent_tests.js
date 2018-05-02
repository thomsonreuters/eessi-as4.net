describe('new empty agent', () => {
    beforeEach('login', () => cy.login())

    it('should have default steps', () => {
        cy.visit('/receive/push')

        const name = 'My new agent'
        cy.getdatacy('new').click()
        cy.getdatacy('name').type(name)
        cy.getdatacy('clone').select('Empty')
        cy.getdatacy('ok').click()

        // Use this receiver because has no settings to configure
        cy.getdatacy('receiver').select('PullRequestReceiver')

        cy.getdatacy('collapse').click()
    })
})