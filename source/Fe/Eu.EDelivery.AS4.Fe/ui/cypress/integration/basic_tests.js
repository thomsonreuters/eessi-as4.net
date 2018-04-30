import jq from "jquery"

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
        cy.fixture('login').then((json) => {
            cy.getdatacy('username').type(json.username)
            cy.getdatacy('password').type(json.password)
            cy.getdatacy('login').click()
            cy.url().should('contain', '/settings/portal')
        })
    })

    it('returns to login after logout', () => {
        cy.getdatacy('logout').click()
        cy.url().should('contain', '/login')
    })
})

describe('page smoke tests', () => {
    beforeEach(() => {
        cy.visit('/login')
        cy.fixture('login').then((json) => cy.login(json.username, json.password))
    })

    it('go through all pages', () => {
        cy.fixture('pages').then((json) => {
            json.pages.forEach((page) => {
                cy.log(`go to ${page.title}`)
                if (page.show !== undefined) {
                    cy.getdatacy(page.show).click()
                }
                cy.getdatacy(page.title).click()
                cy.getdatacy('header').should('contain', page.title)
            })
        })
    })
})

describe('submit agent', () => {
    beforeEach(() => {
        cy.visit('/login')
        cy.fixture('login').then((json) => cy.login(json.username, json.password))
        cy.visit('/submit')
    })

    it('adds submit agent', () => {
        cy.fixture('submit_agent').then((json) => {
            cy.getdatacy('new').click()
            cy.getdatacy('name').type(json.originalName)
            cy.getdatacy('ok').click()
            cy.getdatacy('save').click({ force: true })
            cy.getdatacy('ok').click()
            cy.getdatacy('agents').select(json.originalName)
        })
    })

    it('renames submit agent', () => {
        cy.fixture('submit_agent').then((json) => {
            cy.getdatacy('agents').select(json.originalName)
            cy.getdatacy('rename').click()
            cy.get('div.modal-body input[type=text]').type(json.newName)
            cy.getdatacy('ok').click()
            cy.getdatacy('save').click()
            cy.getdatacy('ok').click()
            cy.getdatacy('agents').select(json.newName)
            cy.getdatacy('delete').click()
            cy.getdatacy('ok').click()
            cy.getdatacy(json.newName).should('not.exist')
        })
    })
})

describe('send pmodes', () => {
    beforeEach(() => {
        cy.visit('/login')
        cy.fixture('login').then((json) => cy.login(json.username, json.password))
        cy.visit('/pmodes/sending')
    })

    it('loads sample pmodes', () => {
        cy.getdatacy('pmodes').within((_) => {
            cy.get('option').should('not.be.empty')
        })
    })

    it('rename/delete sample pmode', () => {
        cy.fixture('send_pmode').then((json) => {
            cy.getdatacy('pmodes').select(json.originalName)
            cy.getdatacy('rename').click()
            cy.get('div.modal-body input[type=text]').type('{selectall}' + json.newName)
            cy.getdatacy('ok').click()
            cy.getdatacy('save').click()
            cy.getdatacy('pmodes').select(json.newName)
            cy.getdatacy('delete').click()
            cy.getdatacy('ok').click()
            cy.getdatacy(json.originalName).should('not.exist')
        })
    })
})

describe('monitoring', () => {
    beforeEach(() => {
        cy.visit('/login')
        cy.fixture('login').then((json) => cy.login(json.username, json.password))
    })

    /** @todo how do we select the right:
     * direction, ebmsMessageType, operation, MEP, and pmodes with generated 'multiselect'?
     * cy.getdatacy('direction').select('Outbound')
     * cy.getdatacy('ebmsMessageType').select('Receipt')
     * cy.getdatacy('status').select('Notified')
     * cy.getdatacy('pmodes').select('01-sample-pmode') */

    it('search for messages', () => {
        cy.visit('/monitor/messages')
        cy.fixture('monitor_message_query').then((json) => {
            cy.getdatacy('ebmsMessageId').type(json.ebmsMessageId)
            cy.getdatacy('fromParty').type(json.fromParty)
            cy.getdatacy('advanced').click()
            cy.getdatacy('mpc').type(json.mpc)
            cy.getdatacy('service').type(json.service)
            cy.getdatacy('action').type(json.action)

            cy.server()
            cy.route('/api/monitor/messages?*').as('query')
            cy.getdatacy('search').click()
            cy.wait('@query')
                .its('url')
                .should('contain', '/monitor/messages?' + 'direction=0&direction=1&' + jq.param(json))
        })
    })

    it('search for exceptions', () => {
        cy.visit('/monitor/exceptions')
        cy.fixture('monitor_exception_query').then((json) => {
            cy.getdatacy('ebmsRefToMessageId').type(json.ebmsRefToMessageId)

            cy.server()
            cy.route('/api/monitor/exceptions?*').as('query')
            cy.getdatacy('search').click()
            cy.wait('@query')
                .its('url')
                .should('contain', '/monitor/exceptions?' + 'page=1&direction=0&direction=1&' + jq.param(json))
        })
    })
})