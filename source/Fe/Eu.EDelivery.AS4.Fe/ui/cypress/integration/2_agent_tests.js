describe('new empty agent', () => {
    beforeEach('login', () => cy.login())

    let createEmptyAgent = ({
        normalPipelineShould,
        errorPipelineShould
    }) => {
        const stubName = 'My new agent'
        cy.getdatacy('new').click()
        cy.getdatacy('name').type(stubName)
        cy.getdatacy('clone').select("Empty")
        cy.getdatacy("ok").click()

        cy.getdatacy('collapse').click()
        cy.getdatacy('normal-pipeline').within(() => {
            cy.getdatacy('step-row').should(normalPipelineShould)
        })
        cy.getdatacy('error-pipeline').within(() => {
            cy.getdatacy('step-row').should(errorPipelineShould)
        })
    }

    it('should have default submit steps', () => {
        cy.visit('/submit')
        createEmptyAgent({
            normalPipelineShould: 'not.be.empty',
            errorPipelineShould: 'not.exist'
        })
    })

    it('should have default receive steps', () => {
        cy.visit('/receive/push')
        createEmptyAgent({
            normalPipelineShould: 'not.be.empty',
            errorPipelineShould: 'not.be.empty'
        })
    })
})