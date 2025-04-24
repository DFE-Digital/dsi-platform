const concat = require("gulp-concat");
const gulp = require("gulp");
const rename = require("gulp-rename");
const uglify = require("gulp-uglify");
const cleanCss = require("gulp-clean-css");

gulp.task("scripts", function () {
  return gulp.src([
    "node_modules/lunr/lunr.min.js",
    "node_modules/@highlightjs/cdn-assets/highlight.min.js",
    "node_modules/@highlightjs/cdn-assets/languages/csharp.min.js",
    "dsi/scripts/*.js",
  ])
    .pipe(concat("app.js"))
    .pipe(uglify())
    .pipe(rename({ extname: ".min.js" }))
    .pipe(gulp.dest("dsi/dist/"));
});

gulp.task("styles", function () {
  return gulp.src([
    "node_modules/@highlightjs/cdn-assets/styles/github.min.css",
    "dsi/styles/*.css",
  ])
    .pipe(concat("app.css"))
    .pipe(cleanCss())
    .pipe(rename({ extname: ".min.css" }))
    .pipe(gulp.dest("dsi/dist/"));
});


gulp.task("default", gulp.parallel("scripts", "styles"));
