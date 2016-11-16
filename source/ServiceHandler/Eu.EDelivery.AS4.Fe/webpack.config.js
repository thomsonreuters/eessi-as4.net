/**
 * @author: @AngularClass
 */

// Look in ./ui/config folder for webpack.dev.js
switch (process.env.NODE_ENV) {
    case 'prod':
    case 'production':
        module.exports = require('./ui/config/webpack.prod')({ env: 'production' });
        break;
    case 'test':
    case 'testing':
        module.exports = require('./ui/config/webpack.test')({ env: 'test' });
        break;
    case 'dev':
    case 'development':
    default:
        module.exports = require('./ui/config/webpack.dev')({ env: 'development' });
}