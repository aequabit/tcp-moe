<?php

namespace MoeApi\Models;

use Illuminate\Database\Eloquent\Model as Eloquent;

class User extends Eloquent {
  public $fillable = [ 'hwid' ];
}
