// Including gulp
var gulp = require('gulp');

// Including plugins
var watch = require('gulp-watch');
var typescript = require('gulp-typescript');
var merge = require('merge2');
var del = require('del');

// Creating typescript project.
// This can have multiple props. Check out gulp-typescript.
var tsProject = typescript.createProject({
  target: 'ES6'
});

// Typescript
gulp.task('typescript', ['clean-typescript'], function() {
  var tsResults = gulp.src('source/**/*.ts')
    .pipe(typescript(tsProject));

  return merge([
    tsResults.dts.pipe(gulp.dest('build/definitions')),
    tsResults.js.pipe(gulp.dest('build/js'))
  ]);
});

//Watch tasks
gulp.task('watch-typescript', ['typescript'], function() {
  watch('source/**/*.ts', function() {
    gulp.start('typescript');
  });
});

gulp.task('watch', ['watch-typescript']);


// Cleaning tasks
gulp.task('clean-typescript', function() {
  del.sync(['build/js/**/*.js']);
});

gulp.task('clean', ['clean-typescript']);

gulp.task('default', ['clean', 'watch']);